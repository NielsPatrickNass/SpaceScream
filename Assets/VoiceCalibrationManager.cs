using UnityEngine;
using Whisper;
using Whisper.Utils; 
using System.Collections;
using System.Threading.Tasks;

public enum CalibrationStage
{
    Idle,
    ListeningForStart, 
    NoiseCalibration,
    WhisperTest,
    NormalTest,
    ShoutTest,
    Completed
}

public class VoiceCalibrationManager : MonoBehaviour
{
    [Header("Components")]
    public VoiceLevelDetector detector;
    public MicrophoneRecord micRecord;
    public WhisperManager whisperManager;

    [Header("Settings")]
    public float testDuration = 6.0f; 

    [Header("Results")]
    public float calibratedWhisperDb;
    public float calibratedNormalDb;
    public float calibratedShoutDb;

    [Header("Feedback")]
    public string realtimeStatus = "Initializing..."; 

    [Header("Debug")]
    public CalibrationStage currentStage = CalibrationStage.Idle;
    public string currentPhaseString; 
    public bool isTestActive;
    
    private bool _latchFailed; 
    private string _targetPhrase;
    private float _testStartTime; 
    private float _accumulatedDb;
    private int _sampleCount;
    private bool _isTransitioning = false;

    // Automatically start listening when the scene loads
    private void Start()
    {
        StartCoroutine(WaitForVoiceCommandLoop());
    }

    private void Update()
    {
        // Only run test logic if we are in an active test stage
        if (isTestActive && currentStage != CalibrationStage.NoiseCalibration && currentStage != CalibrationStage.ListeningForStart)
        {
            float timeElapsed = Time.time - _testStartTime;
            float timeRemaining = testDuration - timeElapsed;

            if (timeRemaining <= 0)
            {
                StopAndEvaluate(); 
                return;
            }

            string timerText = $"({timeRemaining:F1}s)";

            if (timeElapsed < 1.5f) 
            {
                realtimeStatus = $"Get Ready: '{_targetPhrase}'... {timerText}";
                return;
            }

            if (_latchFailed)
            {
                realtimeStatus = $"FAILED: Too Loud! {timerText}";
                return;
            }

            bool isTooLoud = detector.HasViolatedUpperLimit(currentPhaseString);
            bool isTooQuiet = detector.HasViolatedLowerLimit(currentPhaseString);

            if (isTooLoud)
            {
                _latchFailed = true;
                realtimeStatus = $"FAILED: Too Loud! {timerText}";
            }
            else if (isTooQuiet)
            {
                realtimeStatus = (currentPhaseString == "Shout") ? $"LOUDER! {timerText}" : $"Speak Up! {timerText}";
            }
            else
            {
                realtimeStatus = $"Good Level {timerText}"; 
                _accumulatedDb += detector.currentDb;
                _sampleCount++;
            }
        }
    }

    // --- NEW: TOUCHLESS START LOOP ---
    private IEnumerator WaitForVoiceCommandLoop()
    {
        currentStage = CalibrationStage.ListeningForStart;
        
        while (currentStage == CalibrationStage.ListeningForStart)
        {
            realtimeStatus = "Say 'Start Calibration'...";
            
            // 1. Record a short clip (2.5s is enough for the phrase)
            micRecord.StartRecord();
            yield return new WaitForSeconds(2.5f);
            
            // 2. Stop and Transcribe
            // We need to do this manually here to avoid triggering the main StopAndEvaluate logic
            AudioClip clip = micRecord._clip;
            if (clip == null) 
            {
                micRecord.StopRecord();
                yield return null;
                continue;
            }

            // Copy data safely
            int pos = Microphone.GetPosition(micRecord.RecordStartMicDevice);
            if (pos <= 0) pos = clip.samples;
            float[] samples = new float[pos * clip.channels];
            clip.GetData(samples, 0);
            
            AudioClip checkClip = AudioClip.Create("CheckStart", pos, clip.channels, clip.frequency, false);
            checkClip.SetData(samples, 0);
            
            micRecord.StopRecord(); // Reset mic for next loop

            // 3. Check Text (FIXED: Using Task logic instead of await)
            var task = whisperManager.GetTextAsync(checkClip);
            
            // Wait for the task to finish without blocking the Unity Thread
            while (!task.IsCompleted) yield return null; 

            if (task.IsFaulted)
            {
                Debug.LogError("Whisper Task Failed: " + task.Exception);
                Destroy(checkClip);
                yield return null;
                continue;
            }

            var result = task.Result;
            Destroy(checkClip); // Cleanup
            
            string text = result.Result;
            float similarity = TextUtils.CalculateSimilarity("Start Calibration", text);

            if (similarity > 0.65f || text.ToLower().Contains("start calibration"))
            {
                // FOUND IT! Break the loop and start.
                realtimeStatus = "Command Recognized!";
                yield return new WaitForSeconds(1.0f);
                StartAutoCalibration();
                yield break; // Exit loop
            }
            
            yield return null; // Small pause before retrying
        }
    }

    public void StartAutoCalibration()
    {
        if (_isTransitioning) return;
        // If we were listening, this effectively cancels that loop because currentStage changes
        StartCoroutine(RunSequence(CalibrationStage.NoiseCalibration));
    }

    private IEnumerator RunSequence(CalibrationStage stage)
    {
        _isTransitioning = true;
        currentStage = stage;
        isTestActive = false; 

        float countdown = 3.0f;
        string instruction = GetInstructionForStage(stage);
        
        while (countdown > 0)
        {
            realtimeStatus = $"{instruction}\nStarting in {countdown:F1}...";
            yield return null;
            countdown -= Time.deltaTime;
        }

        _isTransitioning = false;

        switch (stage)
        {
            case CalibrationStage.NoiseCalibration:
                currentPhaseString = "Silence";
                micRecord.StartRecord();
                StartCoroutine(RunNoiseRoutine());
                break;
            case CalibrationStage.WhisperTest:
                StartVoiceTestInternal("Whisper", "The coast is clear");
                break;
            case CalibrationStage.NormalTest:
                StartVoiceTestInternal("Normal", "I am speaking normally");
                break;
            case CalibrationStage.ShoutTest:
                StartVoiceTestInternal("Shout", "Incoming fire");
                break;
            case CalibrationStage.Completed:
                realtimeStatus = "ALL TESTS PASSED!\nCalibration Complete.";
                break;
        }
    }

    private string GetInstructionForStage(CalibrationStage stage)
    {
        switch (stage)
        {
            case CalibrationStage.NoiseCalibration: return "Step 1: Silence\nStay quiet to measure room noise.";
            case CalibrationStage.WhisperTest: return "Step 2: Whisper\nWhisper the phrase softly.";
            case CalibrationStage.NormalTest: return "Step 3: Normal\nSpeak in your regular voice.";
            case CalibrationStage.ShoutTest: return "Step 4: Shout\nShout the phrase loudly!";
            default: return "";
        }
    }

    private IEnumerator RunNoiseRoutine()
    {
        float timer = 4.0f;
        while (timer > 0)
        {
            realtimeStatus = $"Calibrating Noise... ({timer:F1}s)";
            yield return null;
            timer -= Time.deltaTime;
        }

        detector.SetBaseline(); 
        micRecord.StopRecord();
        
        realtimeStatus = "Noise Baseline Set!";
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(RunSequence(CalibrationStage.WhisperTest));
    }

    private void StartVoiceTestInternal(string phaseStr, string phrase)
    {
        currentPhaseString = phaseStr;
        _targetPhrase = phrase;
        _latchFailed = false;
        _accumulatedDb = 0f;
        _sampleCount = 0;
        _testStartTime = Time.time;
        isTestActive = true; 
        micRecord.StartRecord();
    }

    public async void StopAndEvaluate()
    {
        isTestActive = false; 
        micRecord.StopRecord();

        if (_latchFailed)
        {
            HandleResult(false, "Failed: Too Loud.");
            return;
        }
        if (_sampleCount < 10)
        {
            HandleResult(false, "Failed: Too Quiet / Short.");
            return;
        }

        realtimeStatus = "Analyzing Audio...";

        // 1. Get raw audio
        AudioClip originalClip = micRecord._clip;
        if (originalClip == null) { HandleResult(false, "Error: No Audio."); return; }

        int currentPos = Microphone.GetPosition(micRecord.RecordStartMicDevice);
        if (currentPos <= 0) currentPos = originalClip.samples;
        
        float[] samples = new float[currentPos * originalClip.channels];
        originalClip.GetData(samples, 0);

        // 2. TRIM SILENCE ALGORITHM
        int trueEndIndex = samples.Length;
        for (int i = samples.Length - 1; i > 0; i--)
        {
            if (Mathf.Abs(samples[i]) > 0.01f) 
            {
                trueEndIndex = i + 1;
                break;
            }
        }
        
        if (trueEndIndex < samples.Length)
        {
            Debug.Log($"Trimming silence: {samples.Length} -> {trueEndIndex} samples.");
        }

        AudioClip trimmedClip = AudioClip.Create("Trimmed", trueEndIndex, originalClip.channels, originalClip.frequency, false);
        
        float[] trimmedSamples = new float[trueEndIndex];
        System.Array.Copy(samples, trimmedSamples, trueEndIndex);
        trimmedClip.SetData(trimmedSamples, 0);

        // 3. Transcribe
        var whisperResult = await whisperManager.GetTextAsync(trimmedClip);
        Destroy(trimmedClip);

        // 4. Check Pronunciation
        float similarity = TextUtils.CalculateSimilarity(_targetPhrase, whisperResult.Result);
        Debug.Log($"Target: '{_targetPhrase}' | Heard: '{whisperResult.Result}' ({similarity*100:F0}%)");

        if (similarity > 0.6f)
        {
            float avgDb = _accumulatedDb / _sampleCount;
            SaveCalibrationResult(avgDb);
            HandleResult(true, $"Success! ({avgDb:F0}dB)");
        }
        else
        {
            HandleResult(false, $"Failed: pronunciation mismatch ({similarity*100:F0}%)");
        }
    }

    private void HandleResult(bool success, string message)
    {
        StartCoroutine(HandleResultRoutine(success, message));
    }

    private IEnumerator HandleResultRoutine(bool success, string message)
    {
        realtimeStatus = message;
        yield return new WaitForSeconds(2.0f);

        if (success)
        {
            CalibrationStage nextStage = currentStage + 1;
            StartCoroutine(RunSequence(nextStage));
        }
        else
        {
            realtimeStatus = "Retrying test...";
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(RunSequence(currentStage));
        }
    }

    private void SaveCalibrationResult(float avgDb)
    {
        switch (currentStage)
        {
            case CalibrationStage.WhisperTest: calibratedWhisperDb = avgDb; break;
            case CalibrationStage.NormalTest: calibratedNormalDb = avgDb; break;
            case CalibrationStage.ShoutTest: calibratedShoutDb = avgDb; break;
        }
    }
}
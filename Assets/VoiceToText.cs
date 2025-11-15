using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;

public class VoiceToText : MonoBehaviour
{
    [Header("Components")]
    public WhisperManager whisper;
    public MicrophoneRecord microphone;

    [Header("Callback")]
    public MonoBehaviour jbehave;  // Drag your jbehave script here

    [Header("UI")]
    public TMPro.TextMeshProUGUI textArea;  // Use this for Legacy Text or InputField.text
                                            // public TMPro.TextMeshProUGUI textArea;  // Or use this for TextMeshPro
                                            // public TMPro.TMP_InputField textInputField;  // Or use this for TMP InputField
                                            // public InputField textInputField;  // Or use this for Legacy InputField

    [Header("Button UI (Optional)")]
    public Button recordButton;
    public TMPro.TextMeshProUGUI buttonText;  // The Text component inside the button
    public Color recordingColor = Color.green;
    public Color idleColor = Color.gray;
    public string recordingText = "Stop Recording";
    public string idleText = "Start Recording";

    [Header("Settings")]
    public bool showPartialResults = true;
    public bool appendResults = false;  // Set to true to keep adding text instead of replacing

    private string _fullText = "";

    private void Awake()
    {
        // Subscribe to events
        microphone.OnRecordStop += OnMicrophoneRecordStop;

        if (showPartialResults)
        {
            whisper.OnNewSegment += OnNewSegment;
        }

        // Subscribe to VAD (voice activity detection) events
        microphone.OnVadChanged += OnVadChanged;
    }

    private void Start()
    {
        // Clear text at start
        UpdateTextArea("Press button to start recording...");

        // Set initial button state
        UpdateButtonVisuals(false);

        // Debug check
        Debug.Log($"Whisper initialized: {whisper != null && whisper.IsLoaded}");
        Debug.Log($"Microphone initialized: {microphone != null}");
    }

    // Call this from a UI button or anywhere in your code
    public void ToggleRecording()
    {
        if (!microphone.IsRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    public void StartRecording()
    {
        UpdateTextArea("Listening...");
        UpdateButtonVisuals(true);
        microphone.StartRecord();
        Debug.Log("Started recording...");
    }

    public void StopRecording()
    {
        microphone.StopRecord();
        UpdateTextArea("Processing...");
        UpdateButtonVisuals(false);
        Debug.Log("Stopped recording, processing audio...");
    }

    private async void OnMicrophoneRecordStop(AudioChunk recordedAudio)
    {
        Debug.Log($"Received audio chunk: {recordedAudio.Data.Length} samples, " +
                  $"{recordedAudio.Frequency}Hz, {recordedAudio.Channels} channels");

        // Get transcription from Whisper
        var result = await whisper.GetTextAsync(
            recordedAudio.Data,
            recordedAudio.Frequency,
            recordedAudio.Channels
        );

        if (result == null)
        {
            UpdateTextArea("Error: Transcription failed");
            Debug.LogError("Transcription failed - result was null");
            return;
        }

        Debug.Log($"Transcription complete: {result.Result}");
        Debug.Log($"Language detected: {result.Language}");

        // Update text area
        if (appendResults)
        {
            _fullText += result.Result + "\n";
            UpdateTextArea(_fullText);
        }
        else
        {
            UpdateTextArea(result.Result);
        }

        // Call the jbehave callback with the transcribed text
        if (jbehave != null)
        {
            jbehave.SendMessage("OnOrderGiven", result.Result, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"Called jbehave.onordergiven with: {result.Result}");
        }
    }

    private void OnNewSegment(WhisperSegment segment)
    {
        // Show partial results while processing (optional)
        if (showPartialResults)
        {
            UpdateTextArea(segment.Text + "...");
            Debug.Log($"Partial: {segment.Text}");
        }
    }

    private void OnVadChanged(bool isSpeechDetected)
    {
        // Visual feedback when speech is detected
        if (microphone.IsRecording && recordButton != null)
        {
            var image = recordButton.GetComponent<Image>();
            if (image != null)
            {
                // Make button brighter when speech detected
                image.color = isSpeechDetected ? Color.Lerp(recordingColor, Color.white, 0.3f) : recordingColor;
            }
        }
        Debug.Log($"Voice detected: {isSpeechDetected}");
    }

    // Helper method to update text in different UI types
    private void UpdateTextArea(string text)
    {
        if (textArea != null)
        {
            textArea.text = text;
        }

        // Uncomment if using InputField
        // if (textInputField != null)
        // {
        //     textInputField.text = text;
        // }
    }

    // Update button color and text
    private void UpdateButtonVisuals(bool isRecording)
    {
        if (recordButton != null)
        {
            var image = recordButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = isRecording ? recordingColor : idleColor;
            }
        }

        if (buttonText != null)
        {
            buttonText.text = isRecording ? recordingText : idleText;
        }
    }

    // Optional: Clear the text
    public void ClearText()
    {
        _fullText = "";
        UpdateTextArea("");
    }
}

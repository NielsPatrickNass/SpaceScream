using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // If using standard UI
using TMPro; // Use this if your texts are TextMeshPro
using Whisper;
using Whisper.Utils;

public class VoiceMenuController : MonoBehaviour
{
    [Header("Dependencies")]
    public WhisperManager whisperManager;
    public MicrophoneRecord micRecord;

    [Header("Menu Text Objects")]
    public TextMeshProUGUI startText;
    public TextMeshProUGUI optionsText;
    public TextMeshProUGUI exitText;

    [Header("Settings")]
    public Color highlightColor = Color.green;
    private Color defaultColor = Color.white;

    private void Start()
    {
        micRecord.OnRecordStop += OnTranscriptionReady;
        micRecord.StartRecord();
        defaultColor = startText.color;
    }

    private async void OnTranscriptionReady(AudioChunk recordedAudio)
    {
        var result = await whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        ProcessCommand(result.Result.ToLower());
        
        // Re-start to keep listening
        micRecord.StartRecord();
    }

    private void ProcessCommand(string text)
{
    text = text.Replace(".", "").Replace("!", "").Trim();
    Debug.Log("Detected: " + text);

    // 1. Reset everything to default first
    ResetColors();

    if (text.Contains("start")) 
    {
        HighlightAndAction(startText, () => SceneManager.LoadScene("SampleScene"));
    }
    else if (text.Contains("option"))
    {
        HighlightAndAction(optionsText, () => Debug.Log("Open Options Logic Here"));
    }
    else if (text.Contains("exit") || text.Contains("quit"))
    {
        HighlightAndAction(exitText, () => Application.Quit());
    }
}

private void ResetColors()
{
    // Set all text objects back to the default color
    startText.color = defaultColor;
    optionsText.color = defaultColor;
    exitText.color = defaultColor;
}

    private void HighlightAndAction(TextMeshProUGUI targetText, System.Action action)
    {
        // Simple visual feedback
        targetText.color = highlightColor;
        // Invoke the actual game logic (loading scene, etc.)
        action.Invoke();
    }
}
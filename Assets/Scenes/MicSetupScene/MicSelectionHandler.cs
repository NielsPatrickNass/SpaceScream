using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MicSelectionHandler : MonoBehaviour
{
    public TMP_Dropdown micDropdown;
    public string nextSceneName = "MainMenu";

    private void Start()
    {
        // 1. Get all available microphone names
        List<string> options = new List<string>(Microphone.devices);
        
        micDropdown.ClearOptions();
        micDropdown.AddOptions(options);

        // 2. Load previously saved mic if it exists
        string savedMic = PlayerPrefs.GetString("UserMic", "");
        int index = options.IndexOf(savedMic);
        if (index != -1) micDropdown.value = index;

        micDropdown.onValueChanged.AddListener(delegate { SaveMicrophone(); });
    }

    public void SaveMicrophone()
    {
        // Save selection to PlayerPrefs so other scenes can read it
        string selectedMic = micDropdown.options[micDropdown.value].text;
        PlayerPrefs.SetString("UserMic", selectedMic);
        Debug.Log($"Microphone saved: {selectedMic}");
    }

    public void ProceedToMenu()
    {
        SaveMicrophone();
        SceneManager.LoadScene(nextSceneName);
    }
}
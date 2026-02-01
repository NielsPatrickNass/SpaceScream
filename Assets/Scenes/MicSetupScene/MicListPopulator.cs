using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MicListPopulator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject optionPrefab; // The CRT List Option we designed
    public ToggleGroup toggleGroup;

    void Start()
    {
        Debug.Log("Checking for microphones...");
        string[] devices = Microphone.devices;
        Debug.Log("Devices found count: " + devices.Length);

        foreach (var device in devices)
        {
            Debug.Log("Device Name: " + device);
        }

        if (devices.Length > 0) PopulateList();
        else Debug.LogWarning("NO MICROPHONES DETECTED BY UNITY!");
        PopulateList();
    }

    void PopulateList()
    {
        // 1. Clear any placeholder items
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Get the actual devices connected to the PC
        string[] devices = Microphone.devices;

        for (int i = 0; i < devices.Length; i++)
        {
            // 3. Spawn the prefab
            GameObject newObj = Instantiate(optionPrefab, transform);
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localScale = Vector3.one;

            // 4. Set the text and Toggle Group
            TextMeshProUGUI label = newObj.GetComponentInChildren<TextMeshProUGUI>();
            label.text = devices[i].ToUpper(); // Retro style usually uses uppercase

            Toggle toggle = newObj.GetComponent<Toggle>();
            toggle.group = toggleGroup;

            // 5. Auto-select the first one (or the saved one)
            string savedMic = PlayerPrefs.GetString("UserMic", "");
            if (devices[i] == savedMic || (string.IsNullOrEmpty(savedMic) && i == 0))
            {
                toggle.isOn = true;
            }

            // 6. Add listener to save when clicked
            string micName = devices[i]; // Local copy for closure
            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn) PlayerPrefs.SetString("UserMic", micName);
            });
        }
    }
}
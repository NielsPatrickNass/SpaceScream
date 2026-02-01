using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CRTListOption : MonoBehaviour
{
    public Toggle myToggle;
    public Image backgroundImage;
    public TextMeshProUGUI labelText;

    // Define your CRT colors here
    private Color phosphorGreen = new Color(0.757f, 0.988f, 0.753f); // Adjust to match your hex
    private Color darkText = new Color32(17, 17, 17, 255);
    private Color transparent = new Color(0, 0, 0, 0);

    void Start()
    {
        // Ensure components are assigned if missed in inspector
        if (myToggle == null) myToggle = GetComponent<Toggle>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (labelText == null) labelText = GetComponentInChildren<TextMeshProUGUI>();

        // Listen for changes
        myToggle.onValueChanged.AddListener(OnToggleValueChanged);
        
        // Initialize state at start
        OnToggleValueChanged(myToggle.isOn);
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            // Selected State: Solid green background, dark text
            backgroundImage.color = phosphorGreen;
            labelText.color = darkText;
        }
        else
        {
            // Normal State: Transparent background, green text
            backgroundImage.color = transparent;
            labelText.color = phosphorGreen;
        }
    }
}
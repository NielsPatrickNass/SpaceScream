using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public RectTransform pauseOverlay;

    public TextMeshProUGUI segmentDisp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ShowPauseOverlay(bool show)
    {
        pauseOverlay.gameObject.SetActive(show);
        segmentDisp.gameObject.SetActive(!show);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            pauseOverlay.gameObject.SetActive(!pauseOverlay.gameObject.activeSelf);
            segmentDisp.gameObject.SetActive(!pauseOverlay.gameObject.activeSelf);
        }
    }
}

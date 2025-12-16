using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    public RectTransform pauseOverlay;

    public TextMeshProUGUI segmentDisp;

    public GameObject notifigRef;

    public Transform notifigHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance= this;
    }

    public void SpawnNotifig(string notifigText)
    {
        GameObject loc = Instantiate(notifigRef, notifigHolder);
        loc.transform.localPosition = Vector3.zero;
        loc.transform.localScale = Vector3.one;
        loc.GetComponent<TextMeshProUGUI>().text = notifigText;
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

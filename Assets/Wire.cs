using UnityEngine;
using UnityEngine.InputSystem;

public class Wire : MonoBehaviour
{
    public LineRenderer Line;
    public Transform EndWire;
    public int WireID;

    public AudioClip SelectSound;
    public AudioClip ConnectSound;
    public AudioClip ErrorSound;

    private Camera cam;
    private bool dragging = false;
    private bool connected = false;
    private Vector3 originalPosition;

    public bool IsConnected => connected;

    void Start()
    {
        cam = Camera.main;
        originalPosition = transform.position;
        WireManager.Instance.Register(this);
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (connected)
                {
                    Disconnect();
                }
                else
                {
                    dragging = true;
                    PlaySound(SelectSound);
                }
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && dragging)
        {
            dragging = false;

            if (!connected)
            {
                ResetPosition();
                PlaySound(ErrorSound);
            }
        }

        if (!dragging) return;

        Vector3 mouse = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse.z = 0;
        SetPosition(mouse);

        if (Vector3.Distance(mouse, EndWire.position) < 0.4f)
        {
            if (WireManager.Instance.CanConnect(this))
            {
                Connect();
            }
            else
            {
                ResetPosition();
                PlaySound(ErrorSound);
                dragging = false;
            }
        }
    }

    void Connect()
    {
        SetPosition(EndWire.position);
        connected = true;
        dragging = false;
        PlaySound(ConnectSound);
        WireManager.Instance.CheckPuzzle();
    }

    public void Disconnect()
    {
        connected = false;
        dragging = false;
        ResetPosition();
    }

    void ResetPosition()
    {
        SetPosition(originalPosition);
    }

    void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        Vector3 diff = pos - Line.transform.position;
        Line.SetPosition(2, diff - new Vector3(.5f, 0, 0));
        Line.SetPosition(3, diff - new Vector3(.15f, 0, 0));
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireManager : MonoBehaviour
{
    public static WireManager Instance;

    public List<Wire> wires = new List<Wire>();
    public AudioClip SuccessSound;

    private Camera cam;
    private bool victorious = false;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public void Register(Wire wire)
    {
        if (!wires.Contains(wire))
            wires.Add(wire);
    }

    void Start()
    {
        ShuffleWires();
    }

    public bool CanConnect(Wire wire)
    {
        WireEnd end = wire.EndWire.GetComponent<WireEnd>();
        return end != null && end.WireID == wire.WireID;
    }

    public void CheckPuzzle()
    {
        if (victorious) return;

        foreach (Wire w in wires)
        {
            if (!w.IsConnected)
                return;
        }

        PuzzleSolved();
    }

    void PuzzleSolved()
    {
        victorious = true;
        PlaySound(SuccessSound);
        Debug.Log("WIRES COMPLETE — DOOR UNLOCKED!");
        // Open door here
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider == null)
            {
                ResetAll();
            }
        }
    }

    void ResetAll()
    {
        victorious = false;

        foreach (Wire w in wires)
            w.Disconnect();

        ShuffleWires();
    }

    void ShuffleWires()
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (Wire w in wires)
            positions.Add(w.EndWire.position);

        foreach (Wire w in wires)
        {
            int i = Random.Range(0, positions.Count);
            w.EndWire.position = positions[i];
            positions.RemoveAt(i);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}

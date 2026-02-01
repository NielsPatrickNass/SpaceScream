using UnityEngine;

public class FloatingShip : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 1f;       // How fast it moves up/down
    public float floatStrength = 10f;   // How far it moves (in pixels)

    [Header("Tilt Settings")]
    public float tiltSpeed = 0.5f;      // How fast it rocks side-to-side
    public float tiltStrength = 2f;     // How much it tilts (degrees)

    private Vector3 startPos;

    void Start()
    {
        // Remember where the ship was placed originally
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 1. Move Up and Down (Sine Wave)
        float newY = startPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatStrength);
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // 2. Tilt Side to Side (Sine Wave)
        float zTilt = Mathf.Sin(Time.time * tiltSpeed) * tiltStrength;
        transform.localRotation = Quaternion.Euler(0, 0, zTilt);
    }
}
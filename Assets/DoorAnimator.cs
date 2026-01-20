using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    [Header("Slide Settings")]
    public Vector3 openOffset = new Vector3(0f, 0f, +1.2f);
    public float speed = 2f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool opening = false;

    [Header("Door Sound")]
    [SerializeField] private AudioClip pressClip;

    void Awake()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + openOffset;
    }

    void Update()
    {
        if (!opening) return;

        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            openPos,
            speed * Time.deltaTime
        );
    }

    public void OpenDoor()
    {
        if (pressClip != null && AudioManager.I != null)
        {
            AudioManager.I.PlaySfx2D(pressClip);
        }

        opening = true;
    }
}
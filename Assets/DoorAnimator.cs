using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    [Header("Slide Settings")]
    public Vector3 openOffset = new Vector3(0f, 0f, +1.2f);
    public float speed = 2f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool opening = false;

    void Awake()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + openOffset;
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            opening ? openPos : closedPos,
            speed * Time.deltaTime
        );
    }

    public void CloseDoor()
    {
        opening = false;
    }

    public void OpenDoor()
    {
        opening = true;
    }
}
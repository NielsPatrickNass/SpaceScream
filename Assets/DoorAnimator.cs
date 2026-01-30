using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorAnimator : MonoBehaviour
{
    [Header("Slide Settings")]
    public Vector3 openOffset = new Vector3(0f, 0f, +1.2f);
    public float speed = 2f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool opening = false;

    public UnityEvent onOpenEvent;

    private Vector3 prevPos;

    public float eventdelay;

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

        if (opening && Vector3.Distance(transform.localPosition, openPos) < 0.1f && Vector3.Distance(prevPos, openPos) > 0.1f)
            StartCoroutine(DelayEvent());

        prevPos = transform.localPosition;


    }

    IEnumerator DelayEvent()
    {
        yield return new WaitForSecondsRealtime(eventdelay);
        onOpenEvent.Invoke();
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
using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    public float openDist;

    public DoorAnimator animator;

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(transform.position, new Vector3(PlayerBehavior.Instance.transform.position.x, transform.position.y, PlayerBehavior.Instance.transform.position.z));
        if (dist < openDist)
        {
            animator.OpenDoor();
        }
        else
            animator.CloseDoor();
    }
}

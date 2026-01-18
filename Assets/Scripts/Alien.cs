using UnityEngine;
using UnityEngine.Events;

public class Alien : MonoBehaviour
{
    public float attackDist;

    public float speed;

    public UnityEvent playerReachedEvent;

    public UnityEvent onEnableScareEvent;

    public Animator anim;

    public void OnEnable()
    {
        if (!PlayerBehavior.Instance.isHiding)
        {
            onEnableScareEvent.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPos = new Vector3(PlayerBehavior.Instance.transform.position.x, transform.position.y, PlayerBehavior.Instance.transform.position.z);
        transform.LookAt(playerPos);
        
        transform.position = Vector3.MoveTowards(transform.position, playerPos, speed);

        bool isCatchingPlayer = Vector3.Distance(transform.position, playerPos) < attackDist;

        if (isCatchingPlayer)
        {
            anim.SetTrigger("attack");
            playerReachedEvent.Invoke();
            PlayerBehavior.Instance.GameOver();
        }
            anim.SetBool("isRunning", !isCatchingPlayer && !PlayerBehavior.Instance.isHiding);
    }
}

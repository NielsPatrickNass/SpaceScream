using System.Collections;
using UnityEngine;

public class LeverController : MonoBehaviour
{
    [Header("Moving part of the lever")]
    public Transform handle;                     

    [Header("Requirement")]
    public ButtonController requiredButton;      // Red Button should be assigned

    [Header("Buttons to lock while lever is down")]
    public ButtonController[] buttonsToLock;     // red and blue buttons should be locked if lever down

    [Header("How far to rotate down/up")]
    public float downAngle = 90f;               
    public float moveDuration = 0.25f;

    private bool isDown = false;
    private Coroutine moveRoutine;
    private Quaternion upRotation;
    private Quaternion downRotation;

    void Start()
    {
        // current pose in editor is in up position
        upRotation = handle.localRotation;

        // compute down rotation by rotating around the local X
        Vector3 upEuler = upRotation.eulerAngles;
        Vector3 downEuler = upEuler + new Vector3(downAngle, 0f, 0f);
        downRotation = Quaternion.Euler(downEuler);

        handle.localRotation = upRotation;
        isDown = false;

        // ! make sure buttons start unlocked
        if (buttonsToLock != null)
        {
            foreach (var btn in buttonsToLock)
            {
                if (btn != null) btn.locked = false;
            }
        }
    }
    // Function for Pull Down the Lever
    public void PullDown()
    {
        if (isDown) return;

        // red button must be pressed first
        if (requiredButton != null && !requiredButton.isPressed)
        {
            Debug.Log("Lever is locked. Please press red button first!");
            return;
        }

        isDown = true;
        StartMove(downRotation);

        // lock all assigned buttons
        if (buttonsToLock != null)
        {
            foreach (var btn in buttonsToLock)
            {
                if (btn != null) btn.locked = true;
            }
        }
    }
    // Function for Push Up the Lever
    public void PushUp()
    {
        if (!isDown) return;

        isDown = false;
        StartMove(upRotation);

        // unlock all assigned buttons
        if (buttonsToLock != null)
        {
            foreach (var btn in buttonsToLock)
            {
                if (btn != null) btn.locked = false;
            }
        }
    }
 
    void StartMove(Quaternion targetRotation)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveLever(targetRotation));
    }
    // Function for Rotation/Moving
    IEnumerator MoveLever(Quaternion targetRotation)
    {
        Quaternion startRotation = handle.localRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            handle.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        handle.localRotation = targetRotation;
    }

    // keyboard test
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) PullDown(); // d for Down
        if (Input.GetKeyDown(KeyCode.U)) PushUp();  // u for Up
    }
}

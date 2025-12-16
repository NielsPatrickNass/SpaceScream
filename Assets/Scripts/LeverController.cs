using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverController : Interactable
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

    public override void Start()
    {
        // wichtig: Interactable registriert sich hier
        base.Start();

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

        // Voice/Interact actions für dieses Puzzle (genau hier drin, kein extra Script)
        if (possibleInteractions == null)
            possibleInteractions = new List<PlayerBehavior.Actions>();

        possibleInteractions.Clear();

        // Diese "verbs" sind absichtlich NICHT States.
        // Sie werden über PerformInteraction ausgeführt.
        possibleInteractions.Add(new PlayerBehavior.Actions("press the red button", "PressRedButton", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("pull the lever down", "PullLeverDown", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("push the lever up", "PushLeverUp", ""));

        // optional: Exit-Commands
        possibleInteractions.Add(new PlayerBehavior.Actions("back", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("exit", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("stop", "back", ""));
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

    // Das ist der entscheidende Teil: Voice-Aktion -> echte Puzzle-Methoden
    public override void PerformInteraction(PlayerBehavior.Actions action, Inventory inventory)
    {
        switch (action.verb)
        {
            case "PressRedButton":
                if (requiredButton == null) return;
                if (requiredButton.locked)
                {
                    Debug.Log("Red button is locked!");
                    return;
                }

                // Du hast keine Methode gezeigt, nur isPressed/locked.
                // Also: Flag setzen. (Wenn du eine Press()-Methode hast, nutz die stattdessen.)
                requiredButton.isPressed = true;
                Debug.Log("Red button pressed (voice).");
                break;

            case "PullLeverDown":
                PullDown();
                break;

            case "PushLeverUp":
                PushUp();
                break;
        }
    }

    // keyboard test (kann bleiben)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) PullDown(); // d for Down
        if (Input.GetKeyDown(KeyCode.U)) PushUp();  // u for Up
    }
}

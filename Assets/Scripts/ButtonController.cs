using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header("Linked button (the opposite one)")]
    public ButtonController otherButton;   // assign the other button 

    [Header("Lock state")]
    public bool locked = false;           // If true, this button cannot be pressed

    [Header("Current state (read-only)")]
    public bool isPressed = false;

    [Header("Press movement settings")]
    public float pressDepth = 0.015f;         
    public Vector3 localPressDirection = new Vector3(0f, 0f, -1f); // local direction of movement, because asset is "false alligned"

    private Vector3 startPos;

    void Start()
    {
        // remember initial local position as unpressed position
        startPos = transform.localPosition;
    }

    void OnMouseDown()
    {
        Press();
    }

    public void Press()
    {
        // If lever locked this button, ignore presses
        if (locked)
        {
            Debug.Log(name + " is locked and cannot be pressed.");
            return;
        }

        Debug.Log(name + " Press() called");

        // reset the other button if there is one
        if (otherButton != null)
        {
            Debug.Log(name + " is resetting " + otherButton.name);
            otherButton.ForceReset();
        }

        // already pressed? then do not move again
        if (isPressed)
            return;

        isPressed = true;

        // move along the local direction
        Vector3 dir = localPressDirection.normalized;
        transform.localPosition = startPos + dir * pressDepth;

        Debug.Log(name + " is now pressed");
    }

    // Reset to start Position
    public void ForceReset()
    {
        isPressed = false;
        transform.localPosition = startPos;
        Debug.Log(name + " reset");
    }
}

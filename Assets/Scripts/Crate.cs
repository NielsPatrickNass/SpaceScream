using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crate : Interactable
{
    public UnityEvent onInteractEvent;

    public bool isOneShot;

    private bool hasbeentriggered;

    public override List<PlayerBehavior.Actions> StartInteraction(PlayerBehavior.Actions lastAction)
    {
        if (isOneShot && hasbeentriggered)
            return base.StartInteraction(lastAction);

        hasbeentriggered = true;

        onInteractEvent.Invoke();

        return base.StartInteraction(lastAction);
    }
}

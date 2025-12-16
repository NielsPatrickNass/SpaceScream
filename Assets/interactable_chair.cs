using UnityEngine;
using System.Collections.Generic;

public class ChairInteractable : Interactable
{
    public Transform sitPoint; // Position + Rotation fürs Hinsetzen

    public override List<PlayerBehavior.Actions> StartInteraction(PlayerBehavior.Actions lastAction)
    {
        var actions = new List<PlayerBehavior.Actions>();
        actions.Add(new PlayerBehavior.Actions(
            "sit down",
            "Sit",
            gameObject.name
        ));
        return actions;
    }

    public override void PerformInteraction(PlayerBehavior.Actions action, Inventory inventory)
    {
        if (action.verb == "Sit")
        {
            var player = FindObjectOfType<PlayerBehavior>();
            player.SitDown(sitPoint);
        }
    }
}

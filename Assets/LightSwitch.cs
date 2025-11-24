using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : Interactable
{
    public GameObject lightToInfluence;

    public override List<PlayerBehavior.Actions> StartInteraction()
    {
        lightToInfluence.SetActive(!lightToInfluence.activeSelf);
        return base.StartInteraction();
    }

}

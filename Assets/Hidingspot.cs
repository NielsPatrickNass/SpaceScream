using System.Collections.Generic;
using UnityEngine;

public class Hidingspot : Interactable
{
    public static List<Hidingspot> hidingspots = new List<Hidingspot>();
    private void OnEnable()
    {
        hidingspots.Add(this);
    }

    private void OnDestroy()
    {
        hidingspots.Remove(this);
    }

    private void OnDisable()
    {
        hidingspots.Remove(this);
    }

    public Transform teleportSpot;

    public static float PlanarDistanceToPlayer(GameObject g)
    {
        return Vector3.Distance(PlayerBehavior.Instance.transform.position, new Vector3(g.transform.position.x, PlayerBehavior.Instance.transform.position.y, g.transform.position.z));
    }

    public static GameObject ClosestHidingSpot()
    {
        int closest = -1;

        for (int i = 0; i < hidingspots.Count; i++) { 

            if (PlanarDistanceToPlayer(hidingspots[i].gameObject) < PlanarDistanceToPlayer(hidingspots[closest].gameObject))
                closest = i;
        }

        if (closest == -1)
            return null;

        return hidingspots[closest].gameObject;
    }

    public override List<PlayerBehavior.Actions> GetCurrentActions()
    {
        return base.GetCurrentActions();
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
    }
}

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

    public static float PlanarDistanceToPlayer(GameObject g)
    {
        return Vector3.Distance(PlayerBehavior.Instance.transform.position, new Vector3(g.transform.position.x, PlayerBehavior.Instance.transform.position.y, g.transform.position.z));
    }

    public override void Awake()
    {
        useSynonyms.Add("Hide");
        base.Awake();
        possibleInteractions.Add(new PlayerBehavior.Actions("stop", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("exit", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("back", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("let us go", "back", ""));
    }

    public static GameObject ClosestHidingSpot()
    {
        int closest = -1;

        for (int i = 0; i < hidingspots.Count; i++) { 

            if (closest == -1 || PlanarDistanceToPlayer(hidingspots[i].gameObject) < PlanarDistanceToPlayer(hidingspots[closest].gameObject))
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

    public override List<PlayerBehavior.Actions> StartInteraction(PlayerBehavior.Actions lastAction)
    {
        if (interactCam != null)
        {
            PlayerBehavior.Instance.cam.gameObject.SetActive(false);
            interactCam.gameObject.SetActive(true);
        }
        return possibleInteractions;
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static PlayerBehavior;

public class Interactable : MonoBehaviour
{
    public Camera interactCam;

    public List<string> synonyms = new List<string>();
    public List<string> useSynonyms = new List<string>();

    public List<PlayerBehavior.Actions> possibleInteractions;

    public static List<Interactable> interactables;

    [System.Serializable]
    public class ActionEventPair
    {
        public string action;
        public UnityEvent eventToTrigger;
        public string requiredItem;
        public bool consumesItem;
        //public bool removeActionAfterSuccess;
    }

    [SerializeField]
    public List<ActionEventPair> actionEventPairs = new List<ActionEventPair>();

    public virtual void Awake()
    {
        if (interactables == null) { 
            interactables = new List<Interactable>();
        }
        interactables.Add(this);

        if (possibleInteractions == null)
            possibleInteractions = new List<PlayerBehavior.Actions>();

        if (possibleInteractions.Count == 0)
            return;
        
        possibleInteractions.Add(new PlayerBehavior.Actions("stop", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("exit", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("back", "back", ""));
        possibleInteractions.Add(new PlayerBehavior.Actions("let us go", "back", ""));
    }

    public List<string> GetPossibleSentences()
    {
        List<string> returnList = new List<string>();

        List<string> allNames = new List<string>();
        allNames.Add(gameObject.name);
        allNames.AddRange(synonyms);
        foreach (string name in allNames)
        {
            returnList.Add("Move to the " + name);
            returnList.Add("Go to the " + name);
            returnList.Add("Use the " + name);
            returnList.Add("Interact with the " + name);
            if (useSynonyms == null)
                useSynonyms = new List<string>();
            foreach (string s in useSynonyms)
                returnList.Add(s + "the " + name);
        }

        return returnList;
    }

    public static List<string> GetPossibleSentencesForAll()
    {
        if (interactables == null)
        {
            interactables = new List<Interactable>();
        }

        List<string> returnList = new List<string>();

        
        foreach (Interactable it in interactables)
        {
            returnList.AddRange(it.GetPossibleSentences());
        }
        return returnList;
    }

    public List<Actions> GetPossibleActions()
    {
        List<Actions> actionsList = new List<Actions>();

        List<string> allNames = new List<string>();
        allNames.Add(gameObject.name);
        allNames.AddRange(synonyms);
        foreach (string name in allNames)
        {
            actionsList.Add(new Actions("Move to the " + name, "MoveTo", gameObject.name));
            actionsList.Add(new Actions("Go to the " + name, "MoveTo", gameObject.name));
            actionsList.Add(new Actions("Use the " + name, "UseInteract", gameObject.name));
            actionsList.Add(new Actions("Interact with the " + name, "UseInteract", gameObject.name));
            if (useSynonyms == null)
                useSynonyms = new List<string>();
            foreach (string s in useSynonyms)
                actionsList.Add(new Actions(s + " " + name, "UseInteract", gameObject.name));
        }

        return actionsList;
    }

    public static List<Actions> GetPossibleActionsForAll()
    {
        List<Actions> actionsList = new List<Actions>();
        foreach (Interactable it in interactables)
        {
            actionsList.AddRange(it.GetPossibleActions()); 
        }
        return actionsList;
    }

    public static Interactable GetInteractable(string interactableToFind)
    {
        foreach (Interactable interactable in interactables)
        {
            if (interactable.name.ToLower().Replace(".", "") == interactableToFind)
                return interactable;
        }
        return null;
    }

    public virtual List<PlayerBehavior.Actions> GetCurrentActions()
    {
        return possibleInteractions;
    }

    public virtual List<PlayerBehavior.Actions> StartInteraction(PlayerBehavior.Actions lastAction)
    {
        Debug.Log("Interact");
        if (interactCam != null)
        {
            PlayerBehavior.Instance.cam.gameObject.SetActive(false);
            interactCam.gameObject.SetActive(true);
        }
        if (actionEventPairs.Count > 0)
            PerformInteraction(lastAction, PlayerBehavior.Instance.inventory);
        if (possibleInteractions.Count == 0)
            EndInteraction();
        
        return possibleInteractions;
    }

    public virtual void PerformInteraction(PlayerBehavior.Actions action, Inventory inventory)
    {
        foreach (ActionEventPair aep in actionEventPairs)
        {
            if (aep.action.ToLower() == action.sentence.ToLower() || aep.action.ToLower() == action.verb.ToLower())
            {
                if (aep.requiredItem == "" || inventory.HasItem(aep.requiredItem))
                    {
                    aep.eventToTrigger.Invoke();
                    if (aep.requiredItem != "" && aep.consumesItem)
                    {
                        inventory.ConsumeItem(aep.requiredItem);
                    }
                }
            }
        }
    }


    public virtual void EndInteraction() {
        if (interactCam != null)
        {
            PlayerBehavior.Instance.cam.gameObject.SetActive(true);
            interactCam.gameObject.SetActive(false);
        }
    }
}

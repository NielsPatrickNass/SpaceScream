using System.Collections.Generic;
using UnityEngine;
using static PlayerBehavior;

public class Interactable : MonoBehaviour
{
    public Camera interactCam;

    public List<string> synonyms = new List<string>();
    public List<string> useSynonyms = new List<string>();

    public List<PlayerBehavior.Actions> possibleInteractions;

    public static List<Interactable> interactables;

    public virtual void Start()
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

    public static List<string> GetPossibleSentences()
    {
        if (interactables == null)
        {
            interactables = new List<Interactable>();
        }

        List<string> returnList = new List<string>();

        
        foreach (Interactable it in interactables)
        {
            List<string> allNames = new List<string>();
            allNames.Add(it.gameObject.name);
            allNames.AddRange(it.synonyms);
            foreach (string name in allNames)
            {
                returnList.Add("Move to the " + name);
                returnList.Add("Go to the " + name);
                returnList.Add("Use the " + name);
                returnList.Add("Interact with the " + name);
                if (it.useSynonyms == null)
                    it.useSynonyms = new List<string>();
                foreach (string s in it.useSynonyms)
                    returnList.Add(s + "the " +  name);
            }
        }
        return returnList;
    }

    public static List<Actions> GetPossibleActions()
    {
        List<Actions> actionsList = new List<Actions>();
        foreach (Interactable it in interactables)
        {
            List<string> allNames = new List<string>();
            allNames.Add(it.gameObject.name);
            allNames.AddRange(it.synonyms);
            foreach (string name in allNames)
            {
                actionsList.Add(new Actions("Move to the " + name, "MoveTo", it.gameObject.name));
                actionsList.Add(new Actions("Go to the " + name, "MoveTo", it.gameObject.name));
                actionsList.Add(new Actions("Use the " + name, "UseInteract", it.gameObject.name));
                actionsList.Add(new Actions("Interact with the " + name, "UseInteract", it.gameObject.name));
                if (it.useSynonyms == null)
                    it.useSynonyms = new List<string>();
                foreach (string s in it.useSynonyms)
                    actionsList.Add(new Actions(s + " " + name, "UseInteract", it.gameObject.name));
            }
        }
        return actionsList;
    }

    public static Interactable GetInteractable(string interactableToFind)
    {
        foreach (Interactable interactable in interactables)
        {
            if (interactable.name == interactableToFind)
                return interactable;
        }
        return null;
    }

    public virtual List<PlayerBehavior.Actions> GetCurrentActions()
    {
        return possibleInteractions;
    }

    public virtual List<PlayerBehavior.Actions> StartInteraction()
    {
        Debug.Log("Interact");
        if (interactCam != null)
            interactCam.gameObject.SetActive(true);

        if (possibleInteractions.Count == 0)
            EndInteraction();
        
        return possibleInteractions;
    }

    public virtual void PerformInteraction(PlayerBehavior.Actions action, Inventory inventory)
    {

    }

    public virtual void EndInteraction() { 
        if (interactCam != null)
        interactCam.gameObject.SetActive(false);
    }
}

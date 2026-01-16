using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using static PlayerBehavior;

public class PickUp : MonoBehaviour
{
    public static List<PickUp> possiblePickUps;

    public List<string> synonyms = new List<string>();

    public static PickUp GetPickUp(string name)
    {
        foreach (PickUp pickup in possiblePickUps)
        {
            if (pickup.gameObject.name.ToLower().Replace(".", "") == name)
                return pickup;
        }

        return null;
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
            returnList.Add("Pick Up the " + name);
        }
        return returnList;
    }

    public static List<string> GetPossibleSentencesForAll()
    {
        if (possiblePickUps == null)
        {
            possiblePickUps = new List<PickUp>();
        }

        List<string> returnList = new List<string>();
        foreach (PickUp pu in possiblePickUps)
        {
            List<string> allNames = new List<string>();
            allNames.Add(pu.gameObject.name);
            allNames.AddRange(pu.synonyms);
            foreach (string name in allNames)
            {
                returnList.Add("Move to the " + name);
                returnList.Add("Go to the " + name);
                returnList.Add("Pick Up the " + name);
            }
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
            actionsList.Add(new Actions("Pick up the " + name, "PickUp", gameObject.name));
        }
        return actionsList;
    }

    public static List<Actions> GetPossibleActionsForAll()
    {
        List<Actions> actionsList = new List<Actions>();
        if (possiblePickUps == null)
        {
            possiblePickUps = new List<PickUp>();
        }

        foreach (PickUp pickup in possiblePickUps)
        {
            List<string> allNames = new List<string>();
            allNames.Add(pickup.gameObject.name);
            allNames.AddRange(pickup.synonyms);
            foreach (string name in allNames)
            {
                actionsList.Add(new Actions("Move to the " + name, "MoveTo", pickup.gameObject.name));
                actionsList.Add(new Actions("Go to the " + name, "MoveTo", pickup.gameObject.name));
                actionsList.Add(new Actions("Pick up the " + name, "PickUp", pickup.gameObject.name));
            }
        }
        return actionsList;
    }

    public void Start()
    {
        if (possiblePickUps == null) { 
            possiblePickUps = new List<PickUp>();
        }

        possiblePickUps.Add(this);
    }

    public void PickedUp()
    {
        possiblePickUps.Remove(this);
    }

}

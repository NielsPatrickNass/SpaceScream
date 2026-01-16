using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerBehavior;

public class RoomInteractableManager : MonoBehaviour
{
    public static RoomInteractableManager instance;

    public Room currentRoom;

    public List<Room> allRooms;

    [System.Serializable]
    public class Room
    {
        public string name;
        public GameObject roomRoot;
        public Camera cam;
    }

    public static bool IsCurrentRoom(string roomToCheck)
    {
        return roomToCheck == instance.currentRoom.name;
    }

    public void Awake()
    {
        instance = this;
    }

    public List<Interactable> GetCurrentRoomInteractables()
    {
        return new List<Interactable>(currentRoom.roomRoot.GetComponentsInChildren<Interactable>());
    }

    public List<PickUp> GetCurrentRoomPickUps()
    {
        return new List<PickUp>(currentRoom.roomRoot.GetComponentsInChildren<PickUp>());
    }

    public List<Actions> GetCurrentRoomActions()
    {
        List<Interactable> currRoomInteractables = GetCurrentRoomInteractables();
        List<PickUp> currRoomPickUps = GetCurrentRoomPickUps();
        List<RoomSwitcher> roomSwitchers = new List<RoomSwitcher>(currentRoom.roomRoot.GetComponentsInChildren<RoomSwitcher>());
        List<Actions> result = new List<Actions>();

        foreach (Interactable it in currRoomInteractables)
        {
            result.AddRange(it.GetPossibleActions());
        }

        foreach (PickUp pu in currRoomPickUps)
        {
            result.AddRange(pu.GetPossibleActions());
        }

        foreach (RoomSwitcher roomSwitcher in roomSwitchers)
        {
                result.Add(new Actions("Move to the " + roomSwitcher.name, "MoveTo", roomSwitcher.name));
                result.Add(new Actions("Go to the " + roomSwitcher.name, "MoveTo", roomSwitcher.name));
            foreach (string s in roomSwitcher.synonyms)
            {
                result.Add(new Actions("Move to the " + s, "MoveTo", roomSwitcher.name));
                result.Add(new Actions("Go to the " + s, "MoveTo", roomSwitcher.name));
            }
        }


        return result;
    }

    public List<string> GetCurrentRoomSentences()
    {
        List<Interactable> currRoomInteractables = GetCurrentRoomInteractables();
        List<PickUp> currRoomPickUps = GetCurrentRoomPickUps();
        List<string> result = new List<string>();

        foreach (Interactable it in currRoomInteractables)
        {
            result.AddRange(it.GetPossibleSentences());
        }
        foreach (PickUp pu in currRoomPickUps)
        {
            result.AddRange(pu.GetPossibleSentences());
        }

        return result;
    }

    public void SetCurrentRoom(GameObject g)
    {
        foreach (Room ri in allRooms)
        {
            if (ri.roomRoot.gameObject.GetInstanceID() == g.GetInstanceID())
            {
                currentRoom = ri;
                ri.cam.gameObject.SetActive(true);
            }
            else
                ri.cam.gameObject.SetActive(false);

        }
    }

    public void SetCurrentRoom(string rname)
    {
        foreach (Room ri in allRooms)
        {
            if (ri.name == rname)
            {
                currentRoom = ri;
                break;
            }
            
        }
    }
}

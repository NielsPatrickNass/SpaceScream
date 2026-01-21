using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
        public UnityEvent OnRoomEnterEvent;
        public UnityEvent OnRoomExitEvent;
    }

    public static bool IsCurrentRoom(string roomToCheck)
    {
        return roomToCheck == instance.currentRoom.name;
    }

    public void Awake()
    {
        instance = this;
        SetCurrentRoom(currentRoom.roomRoot);
    }

    public List<Interactable> GetCurrentRoomInteractables()
    {
        return new List<Interactable>(currentRoom.roomRoot.GetComponentsInChildren<Interactable>());
    }

    public PickUp GetCurrentRoomPickUpByName(string name)
    {
        List<PickUp> loclist = GetCurrentRoomPickUps();

        foreach (PickUp p in loclist)
        {
            if (p.name.ToLower().Replace(".", "") == name)
                return p;
        }
        return null;
    }

    public Hidingspot GetClosestHidingSpotinCurrentRoom()
    {
        Hidingspot closest = null;

        List<Hidingspot> loclist = GetCurrentRoomHidingspots();

        foreach (Hidingspot p in loclist)
        {
            float distCurr = Vector3.Distance(PlayerBehavior.Instance.transform.position, new Vector3(p.transform.position.x, PlayerBehavior.Instance.transform.position.y, p.transform.position.z));
            float distClosest = closest == null ? Mathf.Infinity : Vector3.Distance(PlayerBehavior.Instance.transform.position, new Vector3(closest.transform.position.x, PlayerBehavior.Instance.transform.position.y, closest.transform.position.z));
            if (distCurr < distClosest)
                closest = p;
        }

        return closest;
    }

    public PickUp GetClosestPickUpFromCurrentRoom()
    {
        PickUp closest = null;

        List<PickUp> loclist = GetCurrentRoomPickUps();

        foreach (PickUp p in loclist)
        {
            float distCurr = Vector3.Distance(PlayerBehavior.Instance.transform.position, new Vector3(p.transform.position.x, PlayerBehavior.Instance.transform.position.y, p.transform.position.z));
            float distClosest = closest == null ? Mathf.Infinity : Vector3.Distance(PlayerBehavior.Instance.transform.position, new Vector3(closest.transform.position.x, PlayerBehavior.Instance.transform.position.y, closest.transform.position.z));
            if (distCurr < distClosest)
                closest = p;
        }

        return closest;
    }

    public RoomSwitcher GetCurrentRoomSwitchersByName(string name)
    {
        List<RoomSwitcher> loclist = new List<RoomSwitcher>(currentRoom.roomRoot.GetComponentsInChildren<RoomSwitcher>());

        foreach (RoomSwitcher rs in loclist)
        {
            if (rs.name.ToLower().Replace(".", "") == name)
                return rs;
        }
        return null;
    }

    public Interactable GetCurrentRoomInteractableByName(string name)
    {
        List<Interactable> loclist = GetCurrentRoomInteractables();

        foreach (Interactable i in loclist)
        {
            if (i.name.ToLower().Replace(".", "") == name)
                return i;
        }
        return null;
    }

    public List<PickUp> GetCurrentRoomPickUps()
    {
        return new List<PickUp>(currentRoom.roomRoot.GetComponentsInChildren<PickUp>());
    }

    public List<Hidingspot> GetCurrentRoomHidingspots()
    {
        return new List<Hidingspot>(currentRoom.roomRoot.GetComponentsInChildren<Hidingspot>());
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
                currentRoom.OnRoomExitEvent.Invoke();
                currentRoom = ri;
                currentRoom.OnRoomEnterEvent.Invoke();
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
                currentRoom.OnRoomExitEvent.Invoke();
                currentRoom = ri;
                currentRoom.OnRoomEnterEvent.Invoke();
                ri.cam.gameObject.SetActive(true);
            }
            else
                ri.cam.gameObject.SetActive(false);

        }
    }
}

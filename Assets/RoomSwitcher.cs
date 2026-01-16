using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomSwitcher : MonoBehaviour
{
    public static List<RoomSwitcher> roomSwitchers = new List<RoomSwitcher>();

    public UnityEvent unityEvent;

    private void OnEnable()
    {
        roomSwitchers.Add(this);
    }

    private void OnDisable()
    {
        roomSwitchers.Remove(this);
    }

    public GameObject room;

    public List<string> synonyms;

    public static GameObject FindRoomSwitcher(string name)
    {
        foreach (RoomSwitcher roomSwitcher in roomSwitchers)
        {
            if (roomSwitcher.name.ToLower() == name.ToLower())
                return roomSwitcher.gameObject;

        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            RoomInteractableManager.instance.SetCurrentRoom(room);
            unityEvent.Invoke();
        }
        
    }
}

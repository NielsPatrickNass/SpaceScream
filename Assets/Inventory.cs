using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public float spacing;

    public float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public List<GameObject> GetItems()
    {
        List<GameObject> loclist = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            loclist.Add(transform.GetChild(i).gameObject);
        }
        return loclist;
    }

    public void AddItem(GameObject g)
    {
        g.transform.SetParent(transform, true);
        g.GetComponent<PickUp>().PickedUp();
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = Vector3.zero;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = Vector3.Lerp(transform.GetChild(i).localPosition, new Vector3(-(transform.childCount-1) * spacing /2 + i * spacing, 0, 0), speed);
        }
    }
}

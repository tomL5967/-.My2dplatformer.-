using System.Collections.Generic;
using UnityEngine;

public class ArrayListManager : MonoBehaviour
{
    [SerializeField] private List<string> itemList;

    void Start()
    {
        itemList = new List<string>();
    }

    public void AddItem(string item)
    {
        itemList.Add(item);
        Debug.Log("Added: " + item);
    }

    public void DisplayItems()
    {
        Debug.Log("Current items:");

        foreach (string item in itemList)
        {
            Debug.Log(item);
        }
    }
}
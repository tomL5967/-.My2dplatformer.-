using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform heartContainer;

    // This list keeps track of the hearts on screen.
    private List<GameObject> hearts = new List<GameObject>();

    // This is how many hearts the player starts with.
    public int maxHealth = 5;

    // This runs when the HealthManager object starts.
    void Start()
    {
        InitializeHealth();
    }

    // This creates the starting hearts.
    void InitializeHealth()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);

            // This spaces each heart beside the last one.
            heart.transform.localPosition = new Vector3(i * 50, 0, 0);

            hearts.Add(heart);
        }
    }

    // This removes one heart from the screen.
    public void RemoveHeart()
    {
        if (hearts.Count > 0)
        {
            GameObject heartToRemove = hearts[hearts.Count - 1];
            hearts.RemoveAt(hearts.Count - 1);
            Destroy(heartToRemove);
        }
    }
}
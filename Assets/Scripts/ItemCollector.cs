using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    [SerializeField] private ArrayListManager arrayListManager;
    [SerializeField] private string itemName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            arrayListManager.AddItem(itemName);
            arrayListManager.DisplayItems();

            Destroy(gameObject);
        }
    }
}
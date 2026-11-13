using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;
    public static int gold;

    void Update()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + gold;
        }
    }
}
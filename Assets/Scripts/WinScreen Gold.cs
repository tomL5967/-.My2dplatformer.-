using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinScreenUI : MonoBehaviour
{
    public TMP_Text goldText;
    public string firstLevelScene = "Level1";

    void Start()
    {
        Time.timeScale = 1f;
        goldText.text = "You collected " + GoldUI.gold + " Gold!";
    }

    public void TryAgain()
    {
        GoldUI.gold = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
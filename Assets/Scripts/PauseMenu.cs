using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    void Start()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pausePanel.activeSelf)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
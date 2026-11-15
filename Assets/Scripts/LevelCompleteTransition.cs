using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompleteTransition : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public TMP_Text messageText;
    public string nextSceneName = "Win";
    public float fadeTime = 1f;

    private bool isTransitioning;

    void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0;
            fadePanel.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning || other.GetComponentInParent<Player>() == null)
            return;

        isTransitioning = true;
        StartCoroutine(FadeAndLoadScene());
    }

    IEnumerator FadeAndLoadScene()
    {
        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel is not assigned!");
            yield break;
        }

        fadePanel.gameObject.SetActive(true);

        if (messageText != null)
            messageText.text = "Level Complete!";

        float time = 0;

        while (time < fadeTime)
        {
            time += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Clamp01(time / fadeTime);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(nextSceneName);
    }
}
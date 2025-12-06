using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    public Animator transition;
    public void LoadNextTheme()
    {
        GameManager.Instance.currentThemeIndex++;

        if (GameManager.Instance.currentThemeIndex < 5)
        {
            StartCoroutine(LoadLevel());
        }
        else
        {
            StartCoroutine(LoadEndLevel());
        }
    }
    IEnumerator LoadLevel()
    {
        transition.SetTrigger("FadeOut");

        yield return new WaitForSeconds(1);

        string nextSceneName = "Theme" + (GameManager.Instance.currentThemeIndex + 1);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator LoadEndLevel()
    {
        transition.SetTrigger("FadeOut");

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("EndScene");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    public Text totalScoreText;
    public Text detailText;

    void Start()
    {
        int total = GameManager.Instance.GetTotalScore(); // 0..100
        totalScoreText.text = "Score total : " + total + " / 100";

        detailText.text =
            "Thème 1 : " + GameManager.Instance.themeScores[0] + "/20\n" +
            "Thème 2 : " + GameManager.Instance.themeScores[1] + "/20\n" +
            "Thème 3 : " + GameManager.Instance.themeScores[2] + "/20\n" +
            "Thème 4 : " + GameManager.Instance.themeScores[3] + "/20\n" +
            "Thème 5 : " + GameManager.Instance.themeScores[4] + "/20";
    }

    public void Replay()
    {
        GameManager.Instance.currentThemeIndex = 0;
        for (int i = 0; i < GameManager.Instance.themeScores.Length; i++)
            GameManager.Instance.themeScores[i] = 0;

        SceneManager.LoadScene("Theme1");
    }
}

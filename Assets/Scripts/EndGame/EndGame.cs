using UnityEngine;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Text totalScoreText;   // Texte pour "Score total : X / 100"
    public Text detailScoreText;  // Texte optionnel avec le détail par thème

    void Start()
    {
        int total = GameManager.Instance.GetTotalScore(); // 0..100

        // Score total
        totalScoreText.text = "Score total : " + total + " / 100";

        // Détail par thème (optionnel)
        detailScoreText.text =
            "Culture générale : " + GameManager.Instance.themeScores[0] + "/20\n" +
            "Musique : " + GameManager.Instance.themeScores[1] + "/20\n" +
            "Cinéma : " + GameManager.Instance.themeScores[2] + "/20\n" +
            "Sport : " + GameManager.Instance.themeScores[3] + "/20\n" +
            "Géographie : " + GameManager.Instance.themeScores[4] + "/20";
    }

    public void Replay()
    {
        // Reset des scores et retour au Theme1
        for (int i = 0; i < GameManager.Instance.themeScores.Length; i++)
            GameManager.Instance.themeScores[i] = 0;

        GameManager.Instance.currentThemeIndex = 0;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Theme1");
    }
}

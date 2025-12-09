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

        // Tableau des noms de thèmes (le même que dans QuizManager)
        string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

        // Détail par thème avec les bons noms
        detailScoreText.text = "";
        for (int i = 0; i < 5; i++)
        {
            detailScoreText.text += themes[i] + " : " + GameManager.Instance.themeScores[i] + "/20\n";
        }
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

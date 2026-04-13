using UnityEngine;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Text totalScoreText;   // Texte pour "Score total : X / 100"
    public Text detailScoreText;  // Texte avec le détail par thème
    public Text roleText;
    public Button downloadPDFButton;


    void Start()
    {

        Debug.Log("Historique enregistré.");
        HistorySystem.AddRunToHistory();

        int total = GameManager.Instance.GetTotalScore(); // 0..100
        string roleName = PlayerPrefs.GetString("SelectedRoleName", "Aucun rôle");

        if (total > 100)
        {
            total = 100;
        }

        // Score total
        totalScoreText.text = "Score total : " + total + " / 100";
        if (roleText != null)
            roleText.text = "Rôle : " + roleName;

        // Tableau des noms de thèmes (le même que dans QuizManager)
        string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

        // Détail par thème avec les bons noms
        detailScoreText.text = "";
        for (int i = 0; i < 5; i++)
        {
            detailScoreText.text += themes[i] + " : " + GameManager.Instance.themeScores[i] + "/20\n";
        }
    }

    public void DownloadPDF()
    {
        string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };
        string playerName = PlayerPrefs.GetString("PlayerName", "Inconnu");
        string roleName = PlayerPrefs.GetString("SelectedRoleName", "Aucun rôle");

        int total = GameManager.Instance.GetTotalScore();
        int[] scores = GameManager.Instance.themeScores;

        // Générer le PDF
        PDFGenerator.GenerateScorePDF(playerName, roleName, total, scores, themes);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

}

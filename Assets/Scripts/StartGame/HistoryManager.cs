using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    [Header("Liste")]
    [SerializeField] private Transform contentParent; // Content du ScrollView
    [SerializeField] private GameObject historyItemPrefab; // prefab avec Text + Button

    [Header("Panel Détails")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Text detailTitleText;
    [SerializeField] private Text[] themeLines;
    [SerializeField] private Button dowloadPdfButton;

    private int currentDetailIndex = -1;

    private void OnEnable()
    {
        RefreshHistory();
        detailPanel.SetActive(false);
    }

    public void RefreshHistory()
    {
        // Nettoyer la liste
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        int count = PlayerPrefs.GetInt("History_Count", 0);
        for (int i = 1; i <= count; i++)
        {
            string prefix = "History_" + i + "_";
            if (!PlayerPrefs.HasKey(prefix + "PlayerName"))
                continue; // entrée supprimée ou inexistante

            string playerName = PlayerPrefs.GetString(prefix + "PlayerName", "Inconnu");
            int total = PlayerPrefs.GetInt(prefix + "TotalScore", 0);

            GameObject item = Instantiate(historyItemPrefab, contentParent);
            Text txt = item.GetComponentInChildren<Text>();
            txt.text = playerName + " - " + total + " / 100";

            int capturedIndex = i;
            Button btn = item.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => ShowDetails(capturedIndex));
        }
    }

    public void ShowDetails(int index)
    {
        string prefix = "History_" + index + "_";
        string playerName = PlayerPrefs.GetString(prefix + "PlayerName", "Inconnu");
        int total = PlayerPrefs.GetInt(prefix + "TotalScore", 0);

        detailTitleText.text = playerName + " - " + total + " / 100";

        for (int i = 0; i < themeLines.Length; i++)
        {
            string themeName = PlayerPrefs.GetString(prefix + "ThemeName" + i, "Thème " + (i + 1));
            int scoreTheme = PlayerPrefs.GetInt(prefix + "ScoreTheme" + i, 0);

            themeLines[i].text = themeName + " : " + scoreTheme + " / 20";
        }

        currentDetailIndex = index; // on mémorise quelle run est affichée

        detailPanel.SetActive(true);

        detailPanel.SetActive(true);
    }

    public void CloseDetails()
    {
        detailPanel.SetActive(false);
    }

    public void ClearHistory()
    {
        int count = PlayerPrefs.GetInt("History_Count", 0);

        for (int i = 1; i <= count; i++)
        {
            string prefix = "History_" + i + "_";
            PlayerPrefs.DeleteKey(prefix + "PlayerName");
            PlayerPrefs.DeleteKey(prefix + "TotalScore");
            for (int t = 0; t < 5; t++)
            {
                PlayerPrefs.DeleteKey(prefix + "ScoreTheme" + t);
                PlayerPrefs.DeleteKey(prefix + "ThemeName" + t);
            }
        }

        PlayerPrefs.DeleteKey("History_Count");
        PlayerPrefs.Save();

        RefreshHistory();
        detailPanel.SetActive(false);

        Debug.Log("🗑 Historique vidé");
    }

    public void CloseHistoryDetailPanel()
    {
        detailPanel.SetActive(false);
    }

    public void DownloadCurrentHistoryPDF()
    {
        if (currentDetailIndex <= 0) return;

        string prefix = "History_" + currentDetailIndex + "_";

        string playerName = PlayerPrefs.GetString(prefix + "PlayerName", "Inconnu");
        int total = PlayerPrefs.GetInt(prefix + "TotalScore", 0);

        // reconstruire le tableau des scores de thèmes
        int[] themeScores = new int[5];
        string[] themes = new string[5];
        for (int i = 0; i < 5; i++)
        {
            themeScores[i] = PlayerPrefs.GetInt(prefix + "ScoreTheme" + i, 0);
            themes[i] = PlayerPrefs.GetString(prefix + "ThemeName" + i, "Thème " + (i + 1));
        }

        // appel à ton générateur PDF (on peut surcharger la méthode pour passer aussi les noms de thèmes)
        PDFGenerator.GenerateScorePDF(playerName, total, themeScores, themes);
    }
}

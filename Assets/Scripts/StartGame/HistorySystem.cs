using UnityEngine;

public static class HistorySystem
{
    public static void AddRunToHistory()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Inconnu");

        int totalScore = GameManager.Instance.GetTotalScore(); // 0..100
        int[] themeScores = GameManager.Instance.themeScores;
        string[] themes = GameManager.Instance.themes;

        int count = PlayerPrefs.GetInt("History_Count", 0);
        int index = count + 1; // nouvelle entrée

        string prefix = "History_" + index + "_";

        PlayerPrefs.SetString(prefix + "PlayerName", playerName);
        PlayerPrefs.SetInt(prefix + "TotalScore", totalScore);

        // 5 thèmes
        for (int i = 0; i < themes.Length; i++)
        {
            PlayerPrefs.SetInt(prefix + "ScoreTheme" + i, themeScores[i]);
            PlayerPrefs.SetString(prefix + "ThemeName" + i, themes[i]);
        }

        PlayerPrefs.SetInt("History_Count", index);
        PlayerPrefs.Save();

        Debug.Log("✅ Partie ajoutée à l'historique: index=" + index);
    }
}

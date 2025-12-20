using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

    public int currentThemeIndex = 0;   // 0..4
    public int[] themeScores = new int[5];
    public int questionPerTheme = 20;

    public int currentSaveSlot = 1; // slot choisi pour la prochaine sauvegarde

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPointToCurrentTheme()
    {
        themeScores[currentThemeIndex]++;
    }

    public void AddPointsToCurrentTheme(int amount)
    {
        themeScores[currentThemeIndex] += amount;
    }



    public int GetTotalScore()
    {
        int total = 0;
        for (int i = 0; i < themeScores.Length; i++)
            total += themeScores[i];
        return total;
    }

    public void ResetRun()
    {
        currentThemeIndex = 0;
        for (int i = 0; i < themeScores.Length; i++)
            themeScores[i] = 0;

        PlayerPrefs.DeleteKey("Resume_Theme");
        PlayerPrefs.DeleteKey("Resume_Question");
        PlayerPrefs.DeleteKey("Resume_Score");
    }
}

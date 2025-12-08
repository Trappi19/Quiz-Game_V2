using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SaveGame(string nom, int themeId, int questionId, int score)
    {
        string prefix = $"Save{themeId}_"; // Save1_, Save2_, etc.
        PlayerPrefs.SetString(prefix + "PlayerName", nom);
        PlayerPrefs.SetInt(prefix + "Theme", themeId);
        PlayerPrefs.SetInt(prefix + "Question", questionId);
        PlayerPrefs.SetInt(prefix + "Score", score);
        PlayerPrefs.Save(); // Force écriture disque
        Debug.Log($"Sauvegardé: {nom} T{themeId} Q{questionId} S{score}");
    }

    public bool HasSave(int saveSlot)
    {
        return PlayerPrefs.HasKey($"Save{saveSlot}_PlayerName");
    }

    public (string nom, int theme, int question, int score) LoadGame(int saveSlot)
    {
        string prefix = $"Save{saveSlot}_";
        return (
            PlayerPrefs.GetString(prefix + "PlayerName", "Inconnu"),
            PlayerPrefs.GetInt(prefix + "Theme", 1),
            PlayerPrefs.GetInt(prefix + "Question", 0),
            PlayerPrefs.GetInt(prefix + "Score", 0)
        );
    }
    public void LoadParty(int saveSlot)
    {
        if (!HasSave(saveSlot)) return;

        var (nom, theme, question, score) = LoadGame(saveSlot);
        PlayerPrefs.SetString("PlayerName", nom);

        // Force GameManager
        GameManager.Instance.currentThemeIndex = theme - 1;
        GameManager.Instance.themeScores[GameManager.Instance.currentThemeIndex] = score;
        PlayerPrefs.SetInt("Save" + theme + "_Question", question); // Pour QuizManager

        SceneManager.LoadScene("Theme" + theme);
        Debug.Log($"Chargé Slot{saveSlot}: {nom} T{theme}");
    }

}

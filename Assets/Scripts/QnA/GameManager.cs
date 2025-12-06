using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentThemeIndex = 0;   // 0..4
    public int[] themeScores = new int[5];
    public int questionPerTheme = 20;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddPointToCurrentTheme()
    {
        themeScores[currentThemeIndex]++;
    }

    public int GetTotalScore()
    {
        int total = 0;
        for (int i = 0; i < themeScores.Length; i++)
            total += themeScores[i];
        return total;
    }
}

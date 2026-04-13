using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

    public int currentThemeIndex = 0;   // 0..4
    public int[] themeScores = new int[5];
    public int questionPerTheme = 20;

    public int currentSaveSlot = 1;

    private readonly List<QuestionAndAnswer> answeredQuestions = new List<QuestionAndAnswer>();
    private readonly HashSet<int> answeredQuestionIds = new HashSet<int>();

    public IReadOnlyList<QuestionAndAnswer> AnsweredQuestions => answeredQuestions;

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

    public void RegisterAnsweredQuestion(QuestionAndAnswer question)
    {
        if (question == null)
            return;

        if (question.Id <= 0)
        {
            answeredQuestions.Add(question);
            return;
        }

        if (!answeredQuestionIds.Add(question.Id))
            return;

        answeredQuestions.Add(question);
    }

    public bool TryGetRandomAnsweredQuestion(out QuestionAndAnswer question)
    {
        question = null;

        if (answeredQuestions.Count == 0)
            return false;

        int index = Random.Range(0, answeredQuestions.Count);
        question = answeredQuestions[index];
        return true;
    }

    public void ClearAnsweredQuestionsHistory()
    {
        answeredQuestions.Clear();
        answeredQuestionIds.Clear();
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

        ClearAnsweredQuestionsHistory();

        PlayerPrefs.DeleteKey("SelectedRoleId");
        PlayerPrefs.DeleteKey("SelectedRoleName");
        PlayerPrefs.DeleteKey("Resume_Theme");
        PlayerPrefs.DeleteKey("Resume_Question");
        PlayerPrefs.DeleteKey("Resume_QuestionsAsked");
        PlayerPrefs.DeleteKey("Resume_Score");
        PlayerPrefs.DeleteKey("Resume_QuestionOrder");
    }
}

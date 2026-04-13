using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    private int questionIndex = 0; //compteur séquentiel
    private bool waitingForNextQuestion = false;

    public List<QuestionAndAnswer> QnA = new List<QuestionAndAnswer>();
    public GameObject[] options;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject NextPanel;

    [Header("Boss Question")]
    public Text bossWarningText;

    [Header("Panel + Themes")]
    public Text QuestionTxt;
    public Text ScoreTxt;
    public Text CurrentTheme;
    string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

    [Header("Save System")]
    [SerializeField] public GameObject saveSlotsPanel;   // Save Panel
    [SerializeField]  public GameObject overwritePanel;   // panel "Écraser ?"
    [SerializeField] public Text overwriteText;          // "Écraser la sauvegarde du slot X ?"
    private int pendingSlot = 1;


    int totalQuestions = 0;
    int questionsAskedThisTheme = 0; // Compteur de questions déjà posées

    string connStr = "Server=localhost;Database=quizgame;User ID=root;Password=rootroot;Port=3306;";

    private void Start()
    {
        Debug.Log($"!!! QuizManager.Start() DEBUT questionIndex={questionIndex} !!!");

        CurrentTheme.text = "Theme : " + GameManager.Instance.themes[GameManager.Instance.currentThemeIndex];

        int resumeTheme = PlayerPrefs.GetInt("Resume_Theme", -1);
        int resumeQuestion = PlayerPrefs.GetInt("Resume_Question", 0);
        int resumeQuestionsAsked = PlayerPrefs.GetInt("Resume_QuestionsAsked", -1);
        string resumeQuestionOrder = PlayerPrefs.GetString("Resume_QuestionOrder", string.Empty);

        if (resumeTheme == GameManager.Instance.currentThemeIndex + 1)
        {
            questionIndex = resumeQuestion;
            questionsAskedThisTheme = resumeQuestionsAsked >= 0 ? resumeQuestionsAsked : resumeQuestion;

            Debug.Log($"🔄 Reprise sauvegarde: questionIndex={questionIndex}, questionsAsked={questionsAskedThisTheme}");
        }
        else
        {
            questionIndex = 0;
            questionsAskedThisTheme = 0;
            resumeQuestionOrder = string.Empty;
        }


        LoadQuestionsFromDatabase(resumeQuestionOrder);
        totalQuestions = GameManager.Instance.questionPerTheme;
        CurrentTheme.text = "Theme : " + themes[GameManager.Instance.currentThemeIndex];
        NextPanel.SetActive(false);
        generateQuestion();

        PlayerPrefs.DeleteKey("Resume_Theme");
        PlayerPrefs.DeleteKey("Resume_Question");
        PlayerPrefs.DeleteKey("Resume_QuestionsAsked");
        PlayerPrefs.DeleteKey("Resume_Score");
        PlayerPrefs.DeleteKey("Resume_QuestionOrder");

        if (bossWarningText != null)
            bossWarningText.gameObject.SetActive(false);
    }


    void LoadQuestionsFromDatabase(string forcedQuestionOrder)
    {
        QnA.Clear();

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                Debug.Log("Connexion MariaDB réussie !");

                int themeId = GameManager.Instance.currentThemeIndex + 1;
                int questionsToLoad = Mathf.Min(GameManager.Instance.questionPerTheme, 20);

                List<int> forcedIds = ParseQuestionOrder(forcedQuestionOrder);
                if (forcedIds.Count > 0)
                {
                    bool exactLoaded = TryLoadQuestionsInExactOrder(conn, forcedIds);
                    if (exactLoaded)
                    {
                        Debug.Log($"Thème {themeId} : reprise exacte avec {QnA.Count} questions sauvegardées.");
                        return;
                    }

                    Debug.LogWarning("Impossible de restaurer exactement l'ordre sauvegardé. Fallback sur chargement aléatoire.");
                    QnA.Clear();
                }

                string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };
                string currentTheme = themes[GameManager.Instance.currentThemeIndex];

                string[] queries =
                {
                    @"SELECT q.id, q.question, qc.choice1, qc.choice2, qc.choice3, qc.choice4, qc.correct_choice
                      FROM quiz_question q
                      JOIN quiz_choices qc ON qc.question_index = q.id
                      WHERE q.theme_id = @themeId;",

                    @"SELECT q.id, q.question, qc.choice1, qc.choice2, qc.choice3, qc.choice4, qc.correct_choice
                      FROM quiz_questions q
                      JOIN quiz_choices qc ON qc.question_index = q.id
                      WHERE q.theme_id = @themeId;",

                    @"SELECT q.id, q.question, qc.choice1, qc.choice2, qc.choice3, qc.choice4, qc.correct_choice
                      FROM quiz_questions q
                      JOIN quiz_choices qc ON qc.question_index = q.id
                      WHERE q.theme = @theme;"
                };

                bool loaded = false;

                foreach (string query in queries)
                {
                    List<QuestionAndAnswer> loadedQuestions = new List<QuestionAndAnswer>();
                    HashSet<int> loadedIds = new HashSet<int>();

                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@themeId", themeId);
                            cmd.Parameters.AddWithValue("@theme", currentTheme);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int questionId = Convert.ToInt32(reader["id"]);
                                    if (!loadedIds.Add(questionId))
                                        continue;

                                    QuestionAndAnswer qa = new QuestionAndAnswer
                                    {
                                        Id = questionId,
                                        Question = reader.GetString("question"),
                                        Answers = new string[4]
                                        {
                                            reader.GetString("choice1"),
                                            reader.GetString("choice2"),
                                            reader.GetString("choice3"),
                                            reader.GetString("choice4")
                                        },
                                        CorrectAnswer = reader.GetInt32("correct_choice")
                                    };
                                    loadedQuestions.Add(qa);
                                }
                            }
                        }

                        for (int i = loadedQuestions.Count - 1; i > 0; i--)
                        {
                            int j = UnityEngine.Random.Range(0, i + 1);
                            QuestionAndAnswer temp = loadedQuestions[i];
                            loadedQuestions[i] = loadedQuestions[j];
                            loadedQuestions[j] = temp;
                        }

                        int count = Mathf.Min(questionsToLoad, loadedQuestions.Count);
                        for (int i = 0; i < count; i++)
                            QnA.Add(loadedQuestions[i]);

                        loaded = true;
                        break;
                    }
                    catch (MySqlException)
                    {
                        QnA.Clear();
                    }
                }

                if (!loaded)
                {
                    Debug.LogError("Aucune requête de chargement des questions n'a fonctionné (vérifie le schéma de la BDD).");
                    return;
                }

                Debug.Log($"Thème {themeId} : {QnA.Count} questions uniques chargées aléatoirement (max {questionsToLoad})");
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur MariaDB : " + ex.Message);
            }
        }
    }

    private List<int> ParseQuestionOrder(string raw)
    {
        List<int> ids = new List<int>();
        if (string.IsNullOrWhiteSpace(raw))
            return ids;

        string[] parts = raw.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int id) && id > 0)
                ids.Add(id);
        }

        return ids;
    }

    private string BuildCurrentQuestionOrder()
    {
        List<string> ids = new List<string>();
        for (int i = 0; i < QnA.Count; i++)
        {
            if (QnA[i] != null && QnA[i].Id > 0)
                ids.Add(QnA[i].Id.ToString());
        }

        return string.Join(",", ids);
    }

    private bool TryLoadQuestionsInExactOrder(MySqlConnection conn, List<int> orderedIds)
    {
        string[] singleQuestionQueries =
        {
            @"SELECT q.id, q.question, qc.choice1, qc.choice2, qc.choice3, qc.choice4, qc.correct_choice
              FROM quiz_question q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.id = @id
              LIMIT 1;",

            @"SELECT q.id, q.question, qc.choice1, qc.choice2, qc.choice3, qc.choice4, qc.correct_choice
              FROM quiz_questions q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.id = @id
              LIMIT 1;"
        };

        for (int i = 0; i < orderedIds.Count; i++)
        {
            int id = orderedIds[i];
            bool found = false;

            for (int q = 0; q < singleQuestionQueries.Length; q++)
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(singleQuestionQueries[q], conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                QuestionAndAnswer qa = new QuestionAndAnswer
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Question = reader.GetString("question"),
                                    Answers = new string[4]
                                    {
                                        reader.GetString("choice1"),
                                        reader.GetString("choice2"),
                                        reader.GetString("choice3"),
                                        reader.GetString("choice4")
                                    },
                                    CorrectAnswer = reader.GetInt32("correct_choice")
                                };

                                QnA.Add(qa);
                                found = true;
                            }
                        }
                    }
                }
                catch (MySqlException)
                {
                    // essaye la requête suivante
                }

                if (found)
                    break;
            }

            if (!found)
                return false;
        }

        return QnA.Count == orderedIds.Count;
    }

    bool IsBossQuestion()
    {
        int remaining = GameManager.Instance.questionPerTheme - questionsAskedThisTheme;
        return remaining <= 3; // les 3 dernières questions
    }



    public void SauvegarderDansSlot(int slot)
    {
        int indexToSave = waitingForNextQuestion ? questionIndex : currentQuestion;
        string prefix = "Save" + slot + "_";

        Debug.Log($">>> [Sauvegarder] slot={slot}, currentQuestion={currentQuestion}, indexToSave={indexToSave} <<<");

        PlayerPrefs.SetString(prefix + "PlayerName", PlayerPrefs.GetString("PlayerName", "Inconnu"));
        PlayerPrefs.SetInt(prefix + "Theme", GameManager.Instance.currentThemeIndex + 1);
        PlayerPrefs.SetInt(prefix + "Question", indexToSave);
        PlayerPrefs.SetInt(prefix + "QuestionsAsked", questionsAskedThisTheme);
        PlayerPrefs.SetString(prefix + "QuestionOrder", BuildCurrentQuestionOrder());

        // Tous les scores 5 thèmes
        for (int i = 0; i < 5; i++)
            PlayerPrefs.SetInt(prefix + "ScoreTheme" + i, GameManager.Instance.themeScores[i]);

        PlayerPrefs.Save();

        Debug.Log("✅ SAUVEGARDÉ Slot " + slot);
    }


    public void OuvrirChoixSlot()
    {
        saveSlotsPanel.SetActive(true);
    }

    public void FermerChoixSlot()
    {
        saveSlotsPanel.SetActive(false);
        overwritePanel.SetActive(false);
    }

    public void ChoisirSlot(int slot)
    {
        pendingSlot = slot;

        string prefix = "Save" + slot + "_";
        bool existe = PlayerPrefs.HasKey(prefix + "PlayerName");

        if (existe)
        {
            // Demander confirmation
            overwriteText.text = "Écraser la sauvegarde de l'emplacement " + slot + " ?";
            overwritePanel.SetActive(true);
        }
        else
        {
            // Slot vide -> on sauvegarde direct
            SauvegarderDansSlot(slot);
            saveSlotsPanel.SetActive(false);
        }
    }

    public void ConfirmerOverwriteOui()
    {
        SauvegarderDansSlot(pendingSlot);
        overwritePanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
    }

    public void ConfirmerOverwriteNon()
    {
        overwritePanel.SetActive(false);
    }




    public void retry() // Inutiliser
    {
        // Relancer le même thème (mêmes 20 questions)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        Quizpanel.SetActive(false);
        NextPanel.SetActive(true);

        int themeScore = GameManager.Instance.themeScores[GameManager.Instance.currentThemeIndex];
        ScoreTxt.text = "Score du thème : " + themeScore + " / " + GameManager.Instance.questionPerTheme;
    }

    public void correct()
    {
        Debug.Log($"[correct()] avant: questionIndex={questionIndex}, questionsAsked={questionsAskedThisTheme}");

        if (currentQuestion >= 0 && currentQuestion < QnA.Count)
            GameManager.Instance.RegisterAnsweredQuestion(QnA[currentQuestion]);

        // QuestionsAskedThisTheme compte combien de questions ont déjà été posées
        // Les 3 dernières (sur questionPerTheme) valent 2 points
        int remaining = GameManager.Instance.questionPerTheme - questionsAskedThisTheme;

        if (remaining <= 3)
        {
            // Question boss
            GameManager.Instance.AddPointsToCurrentTheme(2);
            Debug.Log("✅ Question BOSS -> +2 points");
        }
        else
        {
            // Question normale
            GameManager.Instance.AddPointToCurrentTheme();
        }

        questionsAskedThisTheme++;
        waitingForNextQuestion = true;

        StartCoroutine(WaitForNext());

        Debug.Log($"[correct()] après: questionIndex={questionIndex}, questionsAsked={questionsAskedThisTheme}");
    }


    public void wrong()
    {
        if (currentQuestion >= 0 && currentQuestion < QnA.Count)
            GameManager.Instance.RegisterAnsweredQuestion(QnA[currentQuestion]);

        questionsAskedThisTheme++;
        waitingForNextQuestion = true;

        StartCoroutine(WaitForNext());
    }


    IEnumerator WaitForNext()
    {
        yield return new WaitForSeconds(1);
        generateQuestion();
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];
            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;

            if (QnA[currentQuestion].CorrectAnswer == i + 1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }

            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;
        }
    }

    void generateQuestion()
    {
        waitingForNextQuestion = false;
        Debug.Log($">>> generateQuestion() appelé, questionIndex={questionIndex} <<<");

        if (questionIndex >= QnA.Count || questionsAskedThisTheme >= GameManager.Instance.questionPerTheme)
        {
            Debug.Log($"Fin thème {GameManager.Instance.currentThemeIndex + 1}");
            GameOver();
            return;
        }

        currentQuestion = questionIndex;
        QuestionTxt.text = QnA[currentQuestion].Question;
        SetAnswers();

        if (bossWarningText != null)
            bossWarningText.gameObject.SetActive(IsBossQuestion());

        questionIndex++;
    }



    // Action bouton "Continuer" sur le panel de fin de thème
    public void NextTheme()
    {
        GameManager.Instance.currentThemeIndex++;

        if (GameManager.Instance.currentThemeIndex < 5)
        {
            string nextSceneName = "Theme" + (GameManager.Instance.currentThemeIndex + 1);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("EndScene");
        }
    }

    public int QuestionIndex
    {
        get { return questionIndex; }
    }
}   

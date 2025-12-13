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

    public List<QuestionAndAnswer> QnA = new List<QuestionAndAnswer>();
    public GameObject[] options;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject NextPanel;

    public Text QuestionTxt;
    public Text ScoreTxt;
    public Text CurrentTheme;
    string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };


    int totalQuestions = 0;          // Nombre de questions à poser pour CE thème (20)
    int questionsAskedThisTheme = 0; // Compteur de questions déjà posées

    string connStr = "Server=localhost;Database=quizgame;User ID=root;Password=rootroot;Port=3306;";

    private void Start()
    {
        Debug.Log($"!!! QuizManager.Start() DEBUT questionIndex={questionIndex} !!!");

        CurrentTheme.text = "Theme : " + GameManager.Instance.themes[GameManager.Instance.currentThemeIndex];

        int resumeTheme = PlayerPrefs.GetInt("Resume_Theme", -1);
        int resumeQuestion = PlayerPrefs.GetInt("Resume_Question", 0);
        int resumeScore = PlayerPrefs.GetInt("Resume_Score", 0);

        if (resumeTheme == GameManager.Instance.currentThemeIndex + 1)
        {
            questionIndex = resumeQuestion;
            questionsAskedThisTheme = resumeQuestion;
            GameManager.Instance.themeScores[GameManager.Instance.currentThemeIndex] = resumeScore;

            Debug.Log($"🔄 Reprise sauvegarde: questionIndex={questionIndex}, score={resumeScore}");
        }
        else
        {
            questionIndex = 0;
            questionsAskedThisTheme = 0;
        }


        LoadQuestionsFromDatabase();
        totalQuestions = GameManager.Instance.questionPerTheme;
        CurrentTheme.text = "Theme : " + themes[GameManager.Instance.currentThemeIndex];
        NextPanel.SetActive(false);
        generateQuestion();

        // Optionnel : nettoyer les clés de reprise
        PlayerPrefs.DeleteKey("Resume_Theme");
        PlayerPrefs.DeleteKey("Resume_Question");
        PlayerPrefs.DeleteKey("Resume_Score");
    }


    void LoadQuestionsFromDatabase()
    {
        QnA.Clear();

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                Debug.Log("Connexion MariaDB réussie !");

                // On charge SEULEMENT les questions du thème courant
                string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };
                string currentTheme = themes[GameManager.Instance.currentThemeIndex];

                string query = $@"
                SELECT q.id, q.question, qc.choice1, qc.choice2, qc.choice3, qc.choice4, qc.correct_choice
                FROM quiz_questions q
                JOIN quiz_choices qc ON qc.question_index = q.id
                WHERE q.theme = @theme
                ORDER BY q.id ASC;";  // ORDRE FIXE par ID

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@theme", currentTheme);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Debug.Log("currentThemeIndex = " + GameManager.Instance.currentThemeIndex);
                            //Debug.Log("currentTheme = " + currentTheme);
                            //Debug.Log("QnA.Count après chargement = " + QnA.Count);

                            QuestionAndAnswer qa = new QuestionAndAnswer
                            {
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
                        }
                    }
                }
                
                Debug.Log($"Thème {GameManager.Instance.currentThemeIndex + 1} '{currentTheme}' : {QnA.Count} questions chargées");
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur MariaDB : " + ex.Message);
            }
        }
    }

    public void Sauvegarder()
    {
        int indexToSave = currentQuestion;

        Debug.Log($">>> [Sauvegarder] appelé, currentQuestion={currentQuestion}, indexToSave={indexToSave} <<<");

        string prefix = "Save1_";
        PlayerPrefs.SetString(prefix + "PlayerName", PlayerPrefs.GetString("PlayerName", "Inconnu"));
        PlayerPrefs.SetInt(prefix + "Theme", GameManager.Instance.currentThemeIndex + 1);
        PlayerPrefs.SetInt(prefix + "Question", indexToSave);

        // SAUVEGARDE TOUS LES SCORES DES 5 THÈMES
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt(prefix + "ScoreTheme" + i, GameManager.Instance.themeScores[i]);
        }

        PlayerPrefs.Save();

        Debug.Log("✅ SAUVEGARDÉ Slot 1");
    }




    public void retry()
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

        GameManager.Instance.AddPointToCurrentTheme();
        questionsAskedThisTheme++;

        StartCoroutine(WaitForNext());

        Debug.Log($"[correct()] après: questionIndex={questionIndex}, questionsAsked={questionsAskedThisTheme}");
    }

    public void wrong()
    {
        questionsAskedThisTheme++;

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

        questionIndex++;
    }



    // À lier à un bouton "Continuer" sur ton GoPanel
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

    // Ajoutez cette propriété publique à la classe QuizManager pour exposer questionIndex
    public int QuestionIndex
    {
        get { return questionIndex; }
    }
}   

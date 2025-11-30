using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager_p1 : MonoBehaviour
{
    public List<QuestionAndAnswer> QnA = new List<QuestionAndAnswer>();
    public GameObject[] options;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject NextPanel;

    public Text QuestionTxt;
    public Text ScoreTxt;

    int totalQuestions = 0;          // Nombre de questions à poser pour CE thème (20)
    int questionsAskedThisTheme = 0; // Compteur de questions déjà posées

    string connStr = "Server=localhost;Database=quizgame;User ID=root;Password=rootroot;Port=3306;";

    private void Start()
    {
        LoadQuestionsFromDatabase();

        // On limite à 20 questions (ou GameManager.questionPerTheme)
        totalQuestions = GameManager.Instance.questionPerTheme;

        NextPanel.SetActive(false);
        generateQuestion();
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

                // VERSION SIMPLE : on prend toutes les questions dans l'ordre
                // et on s'en sert différemment selon la scène.
                //
                // Si tu veux VRAIMENT filtrer par thème en SQL,
                // on adaptera ce SELECT avec un WHERE theme = 'xxx'.
                string query = @"
                SELECT q.id,
                       q.question,
                       qc.choice1,
                       qc.choice2,
                       qc.choice3,
                       qc.choice4,
                       qc.correct_choice
                FROM quiz_questions q
                JOIN quiz_choices qc ON qc.question_index = q.id
                ORDER BY q.id;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string questionText = reader.GetString("question");
                        string c1 = reader.GetString("choice1");
                        string c2 = reader.GetString("choice2");
                        string c3 = reader.GetString("choice3");
                        string c4 = reader.GetString("choice4");
                        int correct = reader.GetInt32("correct_choice"); // 1..4

                        QuestionAndAnswer qa = new QuestionAndAnswer
                        {
                            Question = questionText,
                            Answers = new string[4] { c1, c2, c3, c4 },
                            CorrectAnswer = correct
                        };

                        QnA.Add(qa);
                    }
                }

                Debug.Log("Questions chargées : " + QnA.Count);
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur MariaDB : " + ex.Message);
            }
        }

        // ICI : si tu veux que chaque scène ne voie que 20 questions,
        // tu peux découper en fonction de currentThemeIndex
        // (par ex. questions 0-19 pour thème 0, 20-39 pour thème 1, etc.)
        int themeIndex = GameManager.Instance.currentThemeIndex; // 0..4
        int startIndex = themeIndex * GameManager.Instance.questionPerTheme;
        int count = GameManager.Instance.questionPerTheme;

        // On sécurise au cas où il y aurait moins de questions en BDD
        if (startIndex < 0) startIndex = 0;
        if (startIndex >= QnA.Count) startIndex = 0;
        if (startIndex + count > QnA.Count)
        {
            count = QnA.Count - startIndex;
        }

        // On ne garde que les questions de ce "bloc" pour ce thème
        QnA = QnA.GetRange(startIndex, count);
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
        // On ajoute un point au thème courant dans le GameManager
        GameManager.Instance.AddPointToCurrentTheme();

        QnA.RemoveAt(currentQuestion);
        questionsAskedThisTheme++;

        StartCoroutine(WaitForNext());
    }

    public void wrong()
    {
        QnA.RemoveAt(currentQuestion);
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
        // Si on a déjà posé 20 questions pour ce thème, on arrête
        if (questionsAskedThisTheme >= GameManager.Instance.questionPerTheme)
        {
            Debug.Log("20 questions du thème terminées");
            GameOver();
            return;
        }

        if (QnA.Count > 0)
        {
            currentQuestion = UnityEngine.Random.Range(0, QnA.Count);

            QuestionTxt.text = QnA[currentQuestion].Question;
            SetAnswers();
        }
        else
        {
            Debug.Log("Plus de questions dans la liste");
            GameOver();
        }
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
}

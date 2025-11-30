using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager_p1: MonoBehaviour
{
    public List<QuestionAndAnswer> QnA = new List<QuestionAndAnswer>();
    public GameObject[] options;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject GoPanel;

    public Text QuestionTxt;
    public Text ScoreTxt;

    int totalQuestions = 0;
    public int score;

    string connStr = "Server=localhost;Database=quizgame;User ID=root;Password=rootroot;Port=3306;";

    private void Start()
    {
        LoadQuestionsFromDatabase();
        totalQuestions = QnA.Count;
        GoPanel.SetActive(false);
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
    }



    public void retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
       Quizpanel.SetActive(false);
       GoPanel.SetActive(true);
       ScoreTxt.text = score + " / " + totalQuestions;
    }

    public void correct()
    {   score += 1;
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitForNext());
    }

    public void wrong()
    {
        QnA.RemoveAt(currentQuestion);
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
        if (QnA.Count > 0)
        {
            currentQuestion = UnityEngine.Random.Range(0, QnA.Count);

            QuestionTxt.text = QnA[currentQuestion].Question;
            SetAnswers();

        }
        else
        {
            Debug.Log("Out of Questions");
            GameOver();
        }
    }
}

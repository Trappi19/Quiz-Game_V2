using System;
using System.Collections.Generic;
using Dapper;
using MySqlConnector;

public class QuizRepository
{
    private readonly string connectionString;

    public QuizRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public List<QuestionAndAnswer> LoadQuestionsByTheme(int themeId, string themeName)
    {
        string[] queries =
        {
            @"SELECT q.id AS Id, q.question AS Question, q.indice AS Hint, qc.choice1 AS Choice1, qc.choice2 AS Choice2, qc.choice3 AS Choice3, qc.choice4 AS Choice4, qc.correct_choice AS CorrectChoice
              FROM quiz_question q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.theme_id = @themeId;",

            @"SELECT q.id AS Id, q.question AS Question, q.indice AS Hint, qc.choice1 AS Choice1, qc.choice2 AS Choice2, qc.choice3 AS Choice3, qc.choice4 AS Choice4, qc.correct_choice AS CorrectChoice
              FROM quiz_questions q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.theme_id = @themeId;",

            @"SELECT q.id AS Id, q.question AS Question, q.indice AS Hint, qc.choice1 AS Choice1, qc.choice2 AS Choice2, qc.choice3 AS Choice3, qc.choice4 AS Choice4, qc.correct_choice AS CorrectChoice
              FROM quiz_questions q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.theme = @theme;"
        };

        object parameters = new { themeId, theme = themeName };

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            for (int i = 0; i < queries.Length; i++)
            {
                try
                {
                    List<QuizQuestionRow> rows = new List<QuizQuestionRow>(conn.Query<QuizQuestionRow>(queries[i], parameters));
                    if (rows.Count == 0)
                        continue;

                    return ToQuestions(rows);
                }
                catch
                {
                }
            }
        }

        return new List<QuestionAndAnswer>();
    }

    public List<QuestionAndAnswer> LoadQuestionsByIdsInOrder(List<int> orderedIds)
    {
        List<QuestionAndAnswer> questions = new List<QuestionAndAnswer>();

        string[] queries =
        {
            @"SELECT q.id AS Id, q.question AS Question, q.indice AS Hint, qc.choice1 AS Choice1, qc.choice2 AS Choice2, qc.choice3 AS Choice3, qc.choice4 AS Choice4, qc.correct_choice AS CorrectChoice
              FROM quiz_question q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.id = @id
              LIMIT 1;",

            @"SELECT q.id AS Id, q.question AS Question, q.indice AS Hint, qc.choice1 AS Choice1, qc.choice2 AS Choice2, qc.choice3 AS Choice3, qc.choice4 AS Choice4, qc.correct_choice AS CorrectChoice
              FROM quiz_questions q
              JOIN quiz_choices qc ON qc.question_index = q.id
              WHERE q.id = @id
              LIMIT 1;"
        };

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            for (int i = 0; i < orderedIds.Count; i++)
            {
                int id = orderedIds[i];
                QuizQuestionRow row = null;

                for (int q = 0; q < queries.Length; q++)
                {
                    try
                    {
                        row = conn.QueryFirstOrDefault<QuizQuestionRow>(queries[q], new { id });
                        if (row != null)
                            break;
                    }
                    catch
                    {
                    }
                }

                if (row == null)
                    return new List<QuestionAndAnswer>();

                questions.Add(ToQuestion(row));
            }
        }

        return questions;
    }

    public QuestionAndAnswer LoadDevOpsQuestionByTheme(int themeId)
    {
        const string query = @"SELECT question_difficile AS Question, choice1 AS Choice1, choice2 AS Choice2, choice3 AS Choice3, choice4 AS Choice4, correct_choice AS CorrectChoice
                               FROM question_difficile
                               WHERE id_theme = @themeId
                               LIMIT 1;";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            QuizQuestionRow row = conn.QueryFirstOrDefault<QuizQuestionRow>(query, new { themeId });
            return row == null ? null : ToQuestion(row);
        }
    }

    public QuestionAndAnswer LoadSecoursQuestionByTheme(int themeId)
    {
        const string query = @"SELECT question_secoure AS Question, choice1 AS Choice1, choice2 AS Choice2, choice3 AS Choice3, choice4 AS Choice4, correct_choice AS CorrectChoice
                               FROM question_secoure
                               WHERE id_theme = @themeId
                               LIMIT 1;";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            QuizQuestionRow row = conn.QueryFirstOrDefault<QuizQuestionRow>(query, new { themeId });
            return row == null ? null : ToQuestion(row);
        }
    }

    private static List<QuestionAndAnswer> ToQuestions(List<QuizQuestionRow> rows)
    {
        List<QuestionAndAnswer> questions = new List<QuestionAndAnswer>();
        HashSet<int> loadedIds = new HashSet<int>();

        for (int i = 0; i < rows.Count; i++)
        {
            QuizQuestionRow row = rows[i];
            if (!loadedIds.Add(row.Id))
                continue;

            questions.Add(ToQuestion(row));
        }

        return questions;
    }

    private static QuestionAndAnswer ToQuestion(QuizQuestionRow row)
    {
        return new QuestionAndAnswer
        {
            Id = row.Id,
            Question = row.Question ?? string.Empty,
            Hint = row.Hint ?? string.Empty,
            Answers = new string[4]
            {
                row.Choice1 ?? string.Empty,
                row.Choice2 ?? string.Empty,
                row.Choice3 ?? string.Empty,
                row.Choice4 ?? string.Empty
            },
            CorrectAnswer = row.CorrectChoice,
            IsDevOpsQuestion = false,
            IsCacheQuestion = false
        };
    }

    private class QuizQuestionRow
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Hint { get; set; }
        public string Choice1 { get; set; }
        public string Choice2 { get; set; }
        public string Choice3 { get; set; }
        public string Choice4 { get; set; }
        public int CorrectChoice { get; set; }
    }
}

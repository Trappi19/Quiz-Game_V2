using System;
using UnityEngine;

[Serializable]
public class QuestionAndAnswer
{
    public int Id;
    public string Question;
    public string[] Answers;
    public int CorrectAnswer;
    public string Hint;
    public bool IsDevOpsQuestion;
}

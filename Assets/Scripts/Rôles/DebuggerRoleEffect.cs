public class DebuggerRoleEffect : IRoleEffect
{
    public bool ShowsSkipButton => true;

    public bool TryUseSkip(QuizManager quizManager)
    {
        if (quizManager == null)
            return false;

        return quizManager.TrySkipQuestionWithPoints(1);
    }
}

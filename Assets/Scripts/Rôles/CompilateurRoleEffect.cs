public class CompilateurRoleEffect : IRoleEffect
{
    public bool ShowsSkipButton => false;
    public bool ShowsHintButton => true;

    public bool TryUseSkip(QuizManager quizManager)
    {
        return false;
    }

    public bool TryUseHint(QuizManager quizManager)
    {
        if (quizManager == null)
            return false;

        return quizManager.TryUseCompilerHint();
    }
}

public class NoRoleEffect : IRoleEffect
{
    public bool ShowsSkipButton => false;
    public bool ShowsHintButton => false;

    public bool TryUseSkip(QuizManager quizManager)
    {
        return false;
    }

    public bool TryUseHint(QuizManager quizManager)
    {
        return false;
    }
}

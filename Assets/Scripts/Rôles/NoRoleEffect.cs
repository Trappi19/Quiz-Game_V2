public class NoRoleEffect : IRoleEffect
{
    public bool ShowsSkipButton => false;

    public bool TryUseSkip(QuizManager quizManager)
    {
        return false;
    }
}

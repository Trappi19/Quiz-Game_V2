public class HackerRoleEffect : IRoleEffect
{
    public bool ShowsSkipButton => true;
    public bool ShowsHintButton => false;

    public bool TryUseSkip(QuizManager quizManager)
    {
        if (quizManager == null)
            return false;

        return quizManager.TryUseHackerHack();
    }

    public bool TryUseHint(QuizManager quizManager)
    {
        return false;
    }
}

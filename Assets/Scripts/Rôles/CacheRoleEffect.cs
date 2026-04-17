public class CacheRoleEffect : IRoleEffect
{
    public bool ShowsSkipButton => true;
    public bool ShowsHintButton => false;

    public bool TryUseSkip(QuizManager quizManager)
    {
        if (quizManager == null)
            return false;

        return quizManager.TryUseCacheReplay();
    }

    public bool TryUseHint(QuizManager quizManager)
    {
        return false;
    }
}

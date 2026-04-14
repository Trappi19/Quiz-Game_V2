public interface IRoleEffect
{
    bool ShowsSkipButton { get; }
    bool ShowsHintButton { get; }

    bool TryUseSkip(QuizManager quizManager);
    bool TryUseHint(QuizManager quizManager);
}

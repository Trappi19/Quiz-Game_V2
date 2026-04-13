public interface IRoleEffect
{
    bool ShowsSkipButton { get; }
    bool TryUseSkip(QuizManager quizManager);
}

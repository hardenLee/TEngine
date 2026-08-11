namespace GameLogic
{
    /// <summary>玩家对藏品做出的处理决定。</summary>
    public enum AntiqueVerdict
    {
        Genuine = 1,
        Fake = 2,
        Special = 3,
    }

    public enum RoundState
    {
        Idle,
        Presenting,
        Judging,
        Feedback,
        Finished,
    }
}

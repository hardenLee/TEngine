namespace GameLogic
{
    /// <summary>玩家对藏品做出的处理决定。</summary>
    public enum EAntiqueVerdict
    {
        Genuine = 1, //真
        Fake = 2, //假
        Special = 3, //特殊
    }

    public enum ERoundState
    {
        Idle,
        Presenting,
        Judging,
        Feedback,
        Finished,
    }
}

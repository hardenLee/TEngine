namespace GameLogic
{
    /// <summary>结算页使用的不可变数据快照。</summary>
    public readonly struct SessionResult
    {
        public string LevelName { get; }
        public int Score { get; }
        public int CorrectCount { get; }
        public int JudgedCount { get; }
        public int MaxCombo { get; }
        public bool ReputationExhausted { get; }

        public int AccuracyPercent => JudgedCount == 0 ? 0 : CorrectCount * 100 / JudgedCount;

        public SessionResult(string levelName, int score, int correctCount, int judgedCount,
            int maxCombo, bool reputationExhausted)
        {
            LevelName = levelName;
            Score = score;
            CorrectCount = correctCount;
            JudgedCount = judgedCount;
            MaxCombo = maxCombo;
            ReputationExhausted = reputationExhausted;
        }
    }
}

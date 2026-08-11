namespace GameLogic
{
    /// <summary>结算页使用的不可变数据快照。</summary>
    public readonly struct SessionResult
    {
        // ResultUI 不再需要反查当前 Session，因此把跳关所需的关卡信息一起带到结算页。
        public int LevelId { get; }
        public string LevelName { get; }
        public int Score { get; }
        public int CorrectCount { get; }
        public int JudgedCount { get; }
        public int MaxCombo { get; }
        public bool ReputationExhausted { get; }
        public bool IsPassed { get; }
        public int NextLevelId { get; }

        public int AccuracyPercent => JudgedCount == 0 ? 0 : CorrectCount * 100 / JudgedCount;
        public bool CanEnterNextLevel => IsPassed && NextLevelId > 0;

        public SessionResult(int levelId, string levelName, int score, int correctCount, int judgedCount,
            int maxCombo, bool reputationExhausted, bool isPassed, int nextLevelId)
        {
            LevelId = levelId;
            LevelName = levelName;
            Score = score;
            CorrectCount = correctCount;
            JudgedCount = judgedCount;
            MaxCombo = maxCombo;
            ReputationExhausted = reputationExhausted;
            IsPassed = isPassed;
            NextLevelId = nextLevelId;
        }
    }
}

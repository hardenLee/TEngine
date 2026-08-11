namespace GameLogic
{
    /// <summary>一次鉴定后提供给 UI 的结果快照。</summary>
    public readonly struct JudgmentResult
    {
        public bool IsCorrect { get; }
        public int ScoreDelta { get; }
        public int ReputationDelta { get; }
        public int ComboAfter { get; }
        public string FeedbackLine { get; }
        public AntiqueVerdict CorrectVerdict { get; }

        public JudgmentResult(bool isCorrect, int scoreDelta, int reputationDelta, int comboAfter,
            string feedbackLine, AntiqueVerdict correctVerdict)
        {
            IsCorrect = isCorrect;
            ScoreDelta = scoreDelta;
            ReputationDelta = reputationDelta;
            ComboAfter = comboAfter;
            FeedbackLine = feedbackLine;
            CorrectVerdict = correctVerdict;
        }
    }
}

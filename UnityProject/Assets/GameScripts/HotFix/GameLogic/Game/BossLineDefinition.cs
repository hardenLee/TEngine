namespace GameLogic
{
    public enum BossLineTrigger
    {
        Correct = 1,
        Wrong = 2,
        Combo = 3,
        LowReputation = 4,
        LowTime = 5,
        Finished = 6,
    }

    /// <summary>由 bossLine 表转换而来的吐槽数据。</summary>
    public sealed class BossLineDefinition
    {
        public int LevelId { get; }
        public BossLineTrigger Trigger { get; }
        public int MinCombo { get; }
        public string Line { get; }
        public string AudioAddress { get; }

        public BossLineDefinition(int levelId, BossLineTrigger trigger, int minCombo, string line, string audioAddress)
        {
            LevelId = levelId;
            Trigger = trigger;
            MinCombo = minCombo;
            Line = line;
            AudioAddress = audioAddress;
        }
    }
}

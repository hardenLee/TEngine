using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>关卡规则与可出货藏品池。</summary>
    public sealed class LevelDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public float DurationSeconds { get; }
        public int InitialReputation { get; }
        public int TargetScore { get; }
        public int PassScore { get; }
        public IReadOnlyList<AntiqueDefinition> Antiques { get; }
        public GameRuleSet Rules { get; }
        public IReadOnlyList<BossLineDefinition> BossLines { get; }

        public LevelDefinition(int id, string name, float durationSeconds, int initialReputation, int targetScore,
            int passScore, IReadOnlyList<AntiqueDefinition> antiques, GameRuleSet rules,
            IReadOnlyList<BossLineDefinition> bossLines)
        {
            Id = id;
            Name = name;
            DurationSeconds = durationSeconds;
            InitialReputation = initialReputation;
            TargetScore = targetScore;
            PassScore = passScore;
            Antiques = antiques;
            Rules = rules;
            BossLines = bossLines;
        }
    }
}

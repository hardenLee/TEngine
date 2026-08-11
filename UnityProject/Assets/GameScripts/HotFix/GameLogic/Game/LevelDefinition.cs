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
        public IReadOnlyList<AntiqueDefinition> Antiques { get; }

        public LevelDefinition(int id, string name, float durationSeconds, int initialReputation,
            IReadOnlyList<AntiqueDefinition> antiques)
        {
            Id = id;
            Name = name;
            DurationSeconds = durationSeconds;
            InitialReputation = initialReputation;
            Antiques = antiques;
        }
    }
}

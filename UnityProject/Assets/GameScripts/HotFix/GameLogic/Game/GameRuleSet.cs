using System;
using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>由 gameRule 表构建的单局数值规则。</summary>
    public sealed class GameRuleSet
    {
        private readonly List<ComboRule> _comboRules;
        public int WrongReputationCost { get; }
        public int FeedbackDurationMs { get; }

        public GameRuleSet(List<ComboRule> comboRules, int wrongReputationCost, int feedbackDurationMs)
        {
            _comboRules = comboRules ?? throw new ArgumentNullException(nameof(comboRules));
            if (_comboRules.Count == 0) throw new ArgumentException("gameRule 表至少需要一条规则。", nameof(comboRules));
            _comboRules.Sort((a, b) => a.ComboNeed.CompareTo(b.ComboNeed));
            WrongReputationCost = Math.Max(0, wrongReputationCost);
            FeedbackDurationMs = Math.Max(0, feedbackDurationMs);
        }

        public int GetComboMultiplier(int combo)
        {
            int multiplier = 1;
            foreach (ComboRule rule in _comboRules)
            {
                if (combo < rule.ComboNeed) break;
                multiplier = rule.ComboMultiplier;
            }
            return multiplier;
        }
    }

    public readonly struct ComboRule
    {
        public int ComboNeed { get; }
        public int ComboMultiplier { get; }

        public ComboRule(int comboNeed, int comboMultiplier)
        {
            ComboNeed = comboNeed;
            ComboMultiplier = comboMultiplier;
        }
    }
}

using System;
using System.Collections.Generic;
using GameConfig;

namespace GameLogic
{
    /// <summary>
    /// 配置业务适配层。玩法/UI 只使用运行时模型，不直接依赖 Luban 自动生成类。
    /// </summary>
    public static class DemoAntiqueCatalog
    {
        public static LevelDefinition LoadLevelOne() => LoadLevel(1);

        public static LevelDefinition LoadLevel(int levelId)
        {
            Tables tables = ConfigSystem.Instance.Tables;
            level levelConfig = tables.Tblevel.Get(levelId);

            var levelAntiques = new List<levelAntique>();
            foreach (levelAntique item in tables.TblevelAntique.DataList)
            {
                if (item.LevelId == levelId) levelAntiques.Add(item);
            }
            levelAntiques.Sort((a, b) => a.SpawnOrder.CompareTo(b.SpawnOrder));

            var antiques = new List<AntiqueDefinition>(levelAntiques.Count);
            foreach (levelAntique item in levelAntiques)
            {
                antique antiqueConfig = tables.Tbantique.Get(item.AntiqueId);
                antiques.Add(new AntiqueDefinition(antiqueConfig.Id, antiqueConfig.Name, antiqueConfig.Desc,
                    antiqueConfig.ImageAddress, ToVerdict(antiqueConfig.Verdict), antiqueConfig.CorrectLine,
                    antiqueConfig.WrongLine, antiqueConfig.BaseScore));
            }

            return new LevelDefinition(levelConfig.Id, levelConfig.Name, levelConfig.DurationSeconds,
                levelConfig.InitialReputation, levelConfig.TargetScore, levelConfig.PassScore, antiques,
                LoadRules(tables), LoadBossLines(tables));
        }

        private static GameRuleSet LoadRules(Tables tables)
        {
            var comboRules = new List<ComboRule>();
            int wrongCost = 1;
            int feedbackDuration = 850;
            foreach (gameRule rule in tables.TbgameRule.DataList)
            {
                comboRules.Add(new ComboRule(rule.ComboNeed, rule.ComboMultiplier));
                wrongCost = rule.WrongReputationCost;
                feedbackDuration = rule.FeedbackDurationMs;
            }
            return new GameRuleSet(comboRules, wrongCost, feedbackDuration);
        }

        private static List<BossLineDefinition> LoadBossLines(Tables tables)
        {
            var lines = new List<BossLineDefinition>();
            foreach (bossLine line in tables.TbbossLine.DataList)
            {
                lines.Add(new BossLineDefinition(line.LevelId, (BossLineTrigger)line.TriggerType,
                    line.MinCombo, line.Line, line.AudioAddress));
            }
            return lines;
        }

        private static AntiqueVerdict ToVerdict(int verdict)
        {
            if (verdict < (int)AntiqueVerdict.Genuine || verdict > (int)AntiqueVerdict.Special)
                throw new ArgumentOutOfRangeException(nameof(verdict), verdict, "antique.verdict 必须为 1、2 或 3。");
            return (AntiqueVerdict)verdict;
        }
    }
}

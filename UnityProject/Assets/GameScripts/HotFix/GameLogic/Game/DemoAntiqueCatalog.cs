using System;
using System.Collections.Generic;
using GameConfig;

namespace GameLogic
{
    /// <summary>
    /// 配置业务适配层。
    /// 调用路径：UI 请求 LoadLevel → ConfigSystem 读取 Luban bytes → 本类把自动生成的
    /// GameConfig.xxx 转成玩法层模型 LevelDefinition/AntiqueDefinition。
    /// 这样 GameSessionManager 不会与 Excel 字段或自动生成代码耦合。
    /// </summary>
    public static class DemoAntiqueCatalog
    {
        public static LevelDefinition LoadLevelOne() => LoadLevel(1);

        public static LevelDefinition LoadLevel(int levelId)
        {
            // 第一次访问 Tables 时，ConfigSystem 会通过资源模块加载所有已注册的 bytes 配置。
            Tables tables = ConfigSystem.Instance.Tables;
            level levelConfig = tables.Tblevel.Get(levelId);

            // levelAntique 是“关卡-藏品”关系表：筛选本关并按策划的出货顺序排列。
            var levelAntiques = new List<levelAntique>();
            foreach (levelAntique item in tables.TblevelAntique.DataList)
            {
                if (item.LevelId == levelId) levelAntiques.Add(item);
            }
            levelAntiques.Sort((a, b) => a.SpawnOrder.CompareTo(b.SpawnOrder));

            var antiques = new List<AntiqueDefinition>(levelAntiques.Count);
            foreach (levelAntique item in levelAntiques)
            {
                // 这里是与 myTestTable.Get(10002) 相同的读表方式，只是 ID 来自关系表。
                antique antiqueConfig = tables.Tbantique.Get(item.AntiqueId);
                antiques.Add(new AntiqueDefinition(antiqueConfig.Id, antiqueConfig.Name, antiqueConfig.Desc,
                    antiqueConfig.ImageAddress, ToVerdict(antiqueConfig.Verdict), antiqueConfig.CorrectLine,
                    antiqueConfig.WrongLine, antiqueConfig.BaseScore));
            }

            // 把五张表的静态数据聚合为“一局游戏需要的完整只读数据”。
            return new LevelDefinition(levelConfig.Id, levelConfig.Name, levelConfig.DurationSeconds,
                levelConfig.InitialReputation, levelConfig.TargetScore, levelConfig.PassScore, levelConfig.UnlockLevelId, antiques,
                LoadRules(tables), LoadBossLines(tables));
        }

        private static GameRuleSet LoadRules(Tables tables)
        {
            // gameRule 的多行用于定义不同连击阈值；扣信誉和反馈时长为全局规则字段。
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
            // 吐槽不会在这里筛关，保留全表，在实际判定时按当前关卡和触发类型选择。
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

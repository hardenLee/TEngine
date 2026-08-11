using System;
using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>
    /// 无 Unity/UI 依赖的单局状态机。
    /// 状态顺序：Idle → Presenting（等待玩家）→ Feedback（展示判定结果）→
    /// Presenting（下一件）或 Finished（结算）。SortingDeskUI 订阅其事件来显示画面。
    /// </summary>
    public sealed class GameSessionManager
    {
        private readonly List<AntiqueDefinition> _queue = new List<AntiqueDefinition>();
        private int _queueIndex;
        private bool _isInputLocked;

        public RoundState State { get; private set; } = RoundState.Idle;
        public LevelDefinition Level { get; private set; }
        public AntiqueDefinition CurrentAntique => _queueIndex < _queue.Count ? _queue[_queueIndex] : null;
        public float RemainingSeconds { get; private set; }
        public int Score { get; private set; }
        public int Reputation { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int CorrectCount { get; private set; }
        public int JudgedCount { get; private set; }
        public int FeedbackDurationMs => Level.Rules.FeedbackDurationMs;

        public event Action<AntiqueDefinition> AntiquePresented;
        public event Action<JudgmentResult> Judged;
        public event Action<SessionResult> Finished;

        public void Start(LevelDefinition level, int randomSeed = 0)
        {
            if (level == null || level.Antiques == null || level.Antiques.Count == 0)
                throw new ArgumentException("关卡必须至少包含一件藏品。", nameof(level));

            Level = level;
            _queue.Clear();
            _queue.AddRange(level.Antiques);
            // 当前先随机出货；若要严格按 levelAntique.SpawnOrder 出货，可移除此 Shuffle 调用。
            Shuffle(_queue, randomSeed == 0 ? Environment.TickCount : randomSeed);
            _queueIndex = 0;
            Score = Combo = MaxCombo = CorrectCount = JudgedCount = 0;
            Reputation = level.InitialReputation;
            RemainingSeconds = level.DurationSeconds;
            _isInputLocked = false;
            State = RoundState.Presenting;
            // 通知 UI 展示第一件货。
            AntiquePresented?.Invoke(CurrentAntique);
        }

        public void Tick(float deltaSeconds)
        {
            // 由 SortingDeskUI.OnUpdate 每帧调用；超时直接进入结算。
            if (State == RoundState.Idle || State == RoundState.Finished || deltaSeconds <= 0f) return;
            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaSeconds);
            if (RemainingSeconds <= 0f) Finish(false);
        }

        public bool SubmitVerdict(AntiqueVerdict verdict)
        {
            // 防止反馈期间或重复点击时重复结算同一件藏品。
            if (State != RoundState.Presenting || _isInputLocked || CurrentAntique == null) return false;
            _isInputLocked = true;
            State = RoundState.Judging;
            JudgedCount++;

            bool correct = verdict == CurrentAntique.CorrectVerdict;
            int scoreDelta = 0;
            int reputationDelta = 0;
            string line;
            if (correct)
            {
                // 倍率读取 gameRule，不在玩法代码中写死连击档位。
                CorrectCount++;
                Combo++;
                MaxCombo = Math.Max(MaxCombo, Combo);
                scoreDelta = CurrentAntique.BaseScore * Level.Rules.GetComboMultiplier(Combo);
                Score += scoreDelta;
                line = GetFeedbackLine(BossLineTrigger.Correct, CurrentAntique.CorrectLine);
            }
            else
            {
                // 信誉扣除值同样由 gameRule 控制；连击在误判后归零。
                Combo = 0;
                reputationDelta = -Level.Rules.WrongReputationCost;
                Reputation = Math.Max(0, Reputation + reputationDelta);
                line = GetFeedbackLine(BossLineTrigger.Wrong, CurrentAntique.WrongLine);
            }

            State = RoundState.Feedback;
            // 只发送不可变结果快照，UI 无需知道具体的计分过程。
            Judged?.Invoke(new JudgmentResult(correct, scoreDelta, reputationDelta, Combo, line, CurrentAntique.CorrectVerdict));
            return true;
        }

        /// <summary>由 UI 在反馈动画完成后调用，派发下一件货。</summary>
        public void ContinueAfterFeedback()
        {
            // 此方法必须在 UI 的反馈动画/停留结束后调用。
            if (State != RoundState.Feedback) return;
            if (Reputation <= 0) { Finish(true); return; }
            _queueIndex++;
            if (_queueIndex >= _queue.Count) { Finish(false); return; }
            _isInputLocked = false;
            State = RoundState.Presenting;
            AntiquePresented?.Invoke(CurrentAntique);
        }

        public SessionResult GetResult() => CreateResult(Reputation <= 0);

        private void Finish(bool reputationExhausted)
        {
            if (State == RoundState.Finished) return;
            State = RoundState.Finished;
            // 将单局状态压缩为结算快照，交由 ResultUI 展示。
            Finished?.Invoke(CreateResult(reputationExhausted));
        }

        private SessionResult CreateResult(bool reputationExhausted)
        {
            // 通关条件由 level.passScore 控制；信誉归零时即使积分足够也视为失败。
            bool isPassed = !reputationExhausted && Score >= Level.PassScore;
            return new SessionResult(Level.Id, Level.Name, Score, CorrectCount, JudgedCount, MaxCombo,
                reputationExhausted, isPassed, Level.UnlockLevelId);
        }

        private string GetFeedbackLine(BossLineTrigger trigger, string fallback)
        {
            // 关卡专属吐槽优先于全局吐槽；两者都没有时使用 antique 表的专属文案。
            foreach (BossLineDefinition line in Level.BossLines)
            {
                if (line.Trigger != trigger || line.MinCombo > Combo) continue;
                if (line.LevelId == Level.Id) return line.Line;
            }
            foreach (BossLineDefinition line in Level.BossLines)
            {
                if (line.Trigger == trigger && line.MinCombo <= Combo && line.LevelId == 0)
                    return line.Line;
            }
            return fallback;
        }

        private static void Shuffle(List<AntiqueDefinition> list, int seed)
        {
            var random = new Random(seed);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>无 Unity/UI 依赖的单局状态机，便于测试和后续接入不同操作方式。</summary>
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
            Shuffle(_queue, randomSeed == 0 ? Environment.TickCount : randomSeed);
            _queueIndex = 0;
            Score = Combo = MaxCombo = CorrectCount = JudgedCount = 0;
            Reputation = level.InitialReputation;
            RemainingSeconds = level.DurationSeconds;
            _isInputLocked = false;
            State = RoundState.Presenting;
            AntiquePresented?.Invoke(CurrentAntique);
        }

        public void Tick(float deltaSeconds)
        {
            if (State == RoundState.Idle || State == RoundState.Finished || deltaSeconds <= 0f) return;
            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaSeconds);
            if (RemainingSeconds <= 0f) Finish(false);
        }

        public bool SubmitVerdict(AntiqueVerdict verdict)
        {
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
                CorrectCount++;
                Combo++;
                MaxCombo = Math.Max(MaxCombo, Combo);
                scoreDelta = CurrentAntique.BaseScore * Level.Rules.GetComboMultiplier(Combo);
                Score += scoreDelta;
                line = GetFeedbackLine(BossLineTrigger.Correct, CurrentAntique.CorrectLine);
            }
            else
            {
                Combo = 0;
                reputationDelta = -Level.Rules.WrongReputationCost;
                Reputation = Math.Max(0, Reputation + reputationDelta);
                line = GetFeedbackLine(BossLineTrigger.Wrong, CurrentAntique.WrongLine);
            }

            State = RoundState.Feedback;
            Judged?.Invoke(new JudgmentResult(correct, scoreDelta, reputationDelta, Combo, line, CurrentAntique.CorrectVerdict));
            return true;
        }

        /// <summary>由 UI 在反馈动画完成后调用，派发下一件货。</summary>
        public void ContinueAfterFeedback()
        {
            if (State != RoundState.Feedback) return;
            if (Reputation <= 0) { Finish(true); return; }
            _queueIndex++;
            if (_queueIndex >= _queue.Count) { Finish(false); return; }
            _isInputLocked = false;
            State = RoundState.Presenting;
            AntiquePresented?.Invoke(CurrentAntique);
        }

        public SessionResult GetResult() => new SessionResult(Level?.Name ?? string.Empty, Score, CorrectCount, JudgedCount, MaxCombo, Reputation <= 0);

        private void Finish(bool reputationExhausted)
        {
            if (State == RoundState.Finished) return;
            State = RoundState.Finished;
            Finished?.Invoke(new SessionResult(Level?.Name ?? string.Empty, Score, CorrectCount, JudgedCount, MaxCombo, reputationExhausted));
        }

        private string GetFeedbackLine(BossLineTrigger trigger, string fallback)
        {
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

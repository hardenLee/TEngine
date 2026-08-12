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
        /// <summary> 当前关卡的出货队列。 Start() 时由 LevelDefinition.Antiques 拷贝而来，后续从这里依次取出藏品。 </summary>
        private readonly List<AntiqueDefinition> _queue = new List<AntiqueDefinition>();

        /// <summary> 当前处理到第几个藏品。 对应 _queue 中的索引。 </summary>
        private int _queueIndex;

        /// <summary> 是否锁定输入。 玩家提交答案后，在反馈阶段禁止再次点击按钮。 </summary>
        private bool _isInputLocked;

        /// <summary> 当前局状态。 Idle → Presenting → Judging → Feedback → Finished </summary>
        public ERoundState State { get; private set; } = ERoundState.Idle;

        /// <summary> 当前关卡配置数据。 包含时间、信誉、目标分数、藏品列表等静态内容。 </summary>
        public LevelDefinition Level { get; private set; }

        /// <summary> 当前展示中的藏品。 根据 _queueIndex 从出货队列中获取。 </summary>
        public AntiqueDefinition CurrentAntique => _queueIndex < _queue.Count ? _queue[_queueIndex] : null;

        /// <summary> 剩余游戏时间（秒）。 每帧由 Tick() 递减。 </summary>
        public float RemainingSeconds { get; private set; }

        /// <summary> 当前累计得分。 答对时根据基础分和连击倍率增加。 </summary>
        public int Score { get; private set; }

        /// <summary> 当前信誉值。 答错时扣除，降到 0 则游戏结束。 </summary>
        public int Reputation { get; private set; }

        /// <summary> 当前连续答对次数。 答错后归零。 </summary>
        public int Combo { get; private set; }

        /// <summary> 本局历史最高连击数。 用于结算展示。 </summary>
        public int MaxCombo { get; private set; }

        /// <summary> 正确判定数量。 玩家答对一件藏品时增加。 </summary>
        public int CorrectCount { get; private set; }

        /// <summary> 已判定数量。 玩家每提交一次答案都会增加。 </summary>
        public int JudgedCount { get; private set; }

        /// <summary> 反馈界面停留时间（毫秒）。 来源于 gameRule 配置表。 </summary>
        public int FeedbackDurationMs => Level.Rules.FeedbackDurationMs;

        /// <summary> 派发新藏品时触发。 UI收到后刷新图片、名称、描述。 </summary>
        public event Action<AntiqueDefinition> AntiquePresented;

        /// <summary> 玩家完成一次鉴定后触发。 UI收到后显示对错、分数变化、吐槽文案。 </summary>
        public event Action<JudgmentResult> Judged;

        /// <summary> 本局结束时触发。 UI收到后打开结算界面。 </summary>
        public event Action<SessionResult> Finished;

        /// <summary>
        /// 开始一局新的游戏。
        /// 负责初始化关卡数据、随机生成出货队列、重置积分与信誉，
        /// 并立即派发第一件藏品给 UI 展示。
        /// 调用方：SortingDeskUI.OnRefresh()
        /// 调用时机：进入关卡时
        /// </summary>
        /// <param name="level">当前关卡配置</param>
        /// <param name="randomSeed">随机种子，0表示使用系统时间</param>
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
            State = ERoundState.Presenting;
            // 通知 UI 展示第一件货。
            AntiquePresented?.Invoke(CurrentAntique);
        }

        /// <summary>
        /// 推进游戏时间。
        /// 由 UI 每帧调用，用于扣减剩余时间。
        /// 当剩余时间归零时自动触发结算。
        /// 调用方：SortingDeskUI.OnUpdate()
        /// 调用频率：每帧一次
        /// </summary>
        /// <param name="deltaSeconds">本帧经过的秒数</param>
        public void Tick(float deltaSeconds)
        {
            // 由 SortingDeskUI.OnUpdate 每帧调用；超时直接进入结算。
            if (State == ERoundState.Idle || State == ERoundState.Finished || deltaSeconds <= 0f)
                return;
            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaSeconds);
            if (RemainingSeconds <= 0f) Finish(false);
        }

        /// <summary>
        /// 提交本次鉴定结果。
        /// 会根据玩家选择与正确答案进行比较，
        /// 计算积分、信誉、连击与反馈文案。
        /// 计算完成后触发 Judged 事件。
        /// 调用方：SortingDeskUI 按钮点击事件
        /// </summary>
        /// <param name="verdict">玩家选择的分类</param>
        /// <returns>
        /// true：提交成功
        /// false：当前状态不允许提交
        /// </returns>
        public bool SubmitVerdict(EAntiqueVerdict verdict)
        {
            // 防止反馈期间或重复点击时重复结算同一件藏品。
            if (State != ERoundState.Presenting || _isInputLocked || CurrentAntique == null)
                return false;
            _isInputLocked = true;
            State = ERoundState.Judging;
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

            State = ERoundState.Feedback;
            // 只发送不可变结果快照，UI 无需知道具体的计分过程。
            Judged?.Invoke(new JudgmentResult(correct, scoreDelta, reputationDelta, Combo, line, CurrentAntique.CorrectVerdict));
            return true;
        }

        /// <summary>
        /// 结束反馈阶段并进入下一件藏品。
        /// 正常流程：
        /// Presenting
        /// SubmitVerdict
        /// Feedback
        /// ContinueAfterFeedback
        /// Presenting(下一件)
        /// 如果信誉归零或所有藏品处理完成，
        /// 则直接进入结算。
        /// 调用方：SortingDeskUI
        /// 调用时机：反馈动画播放结束后
        /// </summary>
        public void ContinueAfterFeedback()
        {
            // 此方法必须在 UI 的反馈动画/停留结束后调用。
            if (State != ERoundState.Feedback)
                return;
            if (Reputation <= 0)
            {
                Finish(true);
                return;
            }
            _queueIndex++;
            if (_queueIndex >= _queue.Count)
            {
                Finish(false);
                return;
            }
            _isInputLocked = false;
            State = ERoundState.Presenting;
            AntiquePresented?.Invoke(CurrentAntique);
        }

        /// <summary>
        /// 获取当前局结算数据快照。
        /// 不会改变游戏状态，
        /// 仅用于查询当前成绩。
        /// 通常用于调试、统计或中途查看结果。
        /// </summary>
        public SessionResult GetResult() => CreateResult(Reputation <= 0);

        /// <summary>
        /// 结束当前对局。
        /// 会切换状态为 Finished，
        /// 生成最终结算数据并触发 Finished 事件。
        /// 触发条件：
        /// 1. 时间耗尽
        /// 2. 信誉归零
        /// 3. 所有藏品处理完成
        /// </summary>
        /// <param name="reputationExhausted">
        /// 是否因为信誉耗尽导致结束
        /// </param>
        private void Finish(bool reputationExhausted)
        {
            if (State == ERoundState.Finished)
                return;
            State = ERoundState.Finished;
            // 将单局状态压缩为结算快照，交由 ResultUI 展示。
            Finished?.Invoke(CreateResult(reputationExhausted));
        }

        /// <summary>
        /// 创建本局结算结果。
        /// 根据当前积分、正确率、信誉等数据
        /// 生成最终 SessionResult。
        /// 同时根据 PassScore 判断是否通关。
        /// </summary>
        /// <param name="reputationExhausted">
        /// 是否因为信誉耗尽导致失败
        /// </param>
        private SessionResult CreateResult(bool reputationExhausted)
        {
            // 通关条件由 level.passScore 控制；信誉归零时即使积分足够也视为失败。
            bool isPassed = !reputationExhausted && Score >= Level.PassScore;
            return new SessionResult(Level.Id, Level.Name, Score, CorrectCount, JudgedCount, MaxCombo,
                reputationExhausted, isPassed, Level.UnlockLevelId);
        }

        /// <summary>
        /// 获取本次判定对应的反馈文案。
        /// 优先级：
        /// 当前关卡专属吐槽
        /// 全局吐槽
        /// antique配置中的默认文案
        /// 用于生成老板吐槽和反馈文本。
        /// </summary>
        /// <param name="trigger">触发类型</param>
        /// <param name="fallback">默认文案</param>
        private string GetFeedbackLine(BossLineTrigger trigger, string fallback)
        {
            // 关卡专属吐槽优先于全局吐槽；两者都没有时使用 antique 表的专属文案。
            foreach (BossLineDefinition line in Level.BossLines)
            {
                if (line.Trigger != trigger || line.MinCombo > Combo)
                    continue;
                if (line.LevelId == Level.Id)
                    return line.Line;
            }
            foreach (BossLineDefinition line in Level.BossLines)
            {
                if (line.Trigger == trigger && line.MinCombo <= Combo && line.LevelId == 0)
                    return line.Line;
            }
            return fallback;
        }

        /// <summary>
        /// 随机打乱藏品队列。
        /// 使用 Fisher-Yates 洗牌算法。
        /// 当前用于生成随机出货顺序。
        /// 如果策划要求固定顺序出货，
        /// 可以移除此步骤。
        /// </summary>
        /// <param name="list">待打乱列表</param>
        /// <param name="seed">随机种子</param>
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

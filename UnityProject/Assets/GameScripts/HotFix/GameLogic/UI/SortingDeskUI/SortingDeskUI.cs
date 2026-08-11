using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 核心分拣工作台；UI 绑定由 SortingDeskUI_Gen.g.cs 自动生成。
    /// 调用链：MainMenuUI 打开本窗口并传入 LevelDefinition → OnRefresh 创建单局 Session
    /// → Session 通过事件驱动本 UI 刷新 → 结算后打开 ResultUI。
    /// </summary>
    [Window(UILayer.UI, location: "SortingDeskUI", fullScreen: true)]
    public partial class SortingDeskUI
    {
        private GameSessionManager _session;
        private bool _isAlive;

        private partial void OnClickGenuineBtn() => Submit(AntiqueVerdict.Genuine);
        private partial void OnClickFakeBtn() => Submit(AntiqueVerdict.Fake);
        private partial void OnClickSpecialBtn() => Submit(AntiqueVerdict.Special);

        protected override void OnRefresh()
        {
            // UserData 由 MainMenuUI 的 ShowUIAsync<SortingDeskUI>(level) 传入。
            // 兜底读取第一关，便于在 Unity 中单独打开该窗口调试。
            var level = UserData as LevelDefinition ?? DemoAntiqueCatalog.LoadLevelOne();
            if (_session != null) Unsubscribe(_session);

            // Session 不操作任何 Unity/UI 组件，只负责本局数据和状态流转。
            _session = new GameSessionManager();
            // UI 订阅 Session 事件；状态变化时由回调更新画面或切换窗口。
            _session.AntiquePresented += PresentAntique;
            _session.Judged += ShowJudgment;
            _session.Finished += ShowResult;
            _isAlive = true;
            m_tmpText.gameObject.SetActive(false);
            _session.Start(level); // 内部会立即派发第一件藏品的 AntiquePresented 事件。
            RefreshHud();
        }

        protected override void OnUpdate()
        {
            // TEngine 每帧调用 UIWindow.OnUpdate；此处把帧时间交给 Session 扣除倒计时。
            if (_session == null || _session.State == RoundState.Finished) return;
            _session.Tick(Time.unscaledDeltaTime);
            RefreshHud();
        }

        protected override void OnDestroy()
        {
            _isAlive = false;
            if (_session != null) Unsubscribe(_session);
            base.OnDestroy();
        }

        private void Submit(AntiqueVerdict verdict)
        {
            // 仅 Session 处于 Presenting 时才会受理；返回 true 代表本次点击有效。
            if (_session != null && _session.SubmitVerdict(verdict)) SetButtonsInteractable(false);
        }

        private void PresentAntique(AntiqueDefinition antique)
        {
            // 由 Session.Start 或 ContinueAfterFeedback 触发，展示当前待鉴定藏品。
            m_tmpName.text = antique.Name;
            m_tmpDescription.text = antique.Description;
            m_imgImage.SetSprite(antique.ImageAddress);
            m_tmpText.gameObject.SetActive(false);
            SetButtonsInteractable(true);
            RefreshHud();
        }

        private void ShowJudgment(JudgmentResult result)
        {
            // SubmitVerdict 完成计算后触发。这里仅展示结果，积分/信誉已经在 Session 内更新。
            string prefix = result.IsCorrect ? "鉴定正确！" : "鉴定失误！";
            string score = result.IsCorrect ? $" +{result.ScoreDelta}" : $" 信誉 {result.ReputationDelta}";
            m_tmpText.text = $"{prefix}{score}\n{result.FeedbackLine}";
            m_tmpText.gameObject.SetActive(true);
            ContinueAfterFeedbackAsync().Forget();
            RefreshHud();
        }

        private async UniTaskVoid ContinueAfterFeedbackAsync()
        {
            // 停留时长来自 gameRule.FeedbackDurationMs；避免把数值写死在 UI。
            await UniTask.Delay(_session.FeedbackDurationMs, ignoreTimeScale: true);
            if (!_isAlive || _session == null || _session.State != RoundState.Feedback) return;
            // 反馈结束后才允许发下一件；若信誉归零或队列结束，Session 会触发 Finished。
            _session.ContinueAfterFeedback();
        }

        private void ShowResult(SessionResult result)
        {
            if (!_isAlive) return;
            // 结算数据作为 UserData 传给 ResultUI；关闭工作台会同时取消其事件订阅。
            GameModule.UI.ShowUIAsync<ResultUI>(result);
            Close();
        }

        private void RefreshHud()
        {
            if (_session == null) return;
            m_tmpLevel.text = _session.Level.Name;
            m_tmpTimer.text = $"{Mathf.CeilToInt(_session.RemainingSeconds):00}s";
            m_tmpScore.text = $"分数 {_session.Score}";
            m_tmpReputation.text = $"信誉 {_session.Reputation}";
            m_tmpCombo.text = $"连击 x{_session.Combo}";
        }

        private void SetButtonsInteractable(bool value)
        {
            m_btnGenuine.interactable = value;
            m_btnFake.interactable = value;
            m_btnSpecial.interactable = value;
        }

        private void Unsubscribe(GameSessionManager session)
        {
            // 窗口关闭/重新刷新时必须解除订阅，防止旧 Session 回调已销毁的 UI。
            session.AntiquePresented -= PresentAntique;
            session.Judged -= ShowJudgment;
            session.Finished -= ShowResult;
        }
    }
}

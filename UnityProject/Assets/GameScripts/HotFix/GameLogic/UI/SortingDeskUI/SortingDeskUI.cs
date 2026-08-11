using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>核心分拣工作台；UI 绑定由 SortingDeskUI_Gen.g.cs 自动生成。</summary>
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
            var level = UserData as LevelDefinition ?? DemoAntiqueCatalog.LoadLevelOne();
            if (_session != null) Unsubscribe(_session);

            _session = new GameSessionManager();
            _session.AntiquePresented += PresentAntique;
            _session.Judged += ShowJudgment;
            _session.Finished += ShowResult;
            _isAlive = true;
            m_tmpText.gameObject.SetActive(false);
            _session.Start(level);
            RefreshHud();
        }

        protected override void OnUpdate()
        {
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
            if (_session != null && _session.SubmitVerdict(verdict)) SetButtonsInteractable(false);
        }

        private void PresentAntique(AntiqueDefinition antique)
        {
            m_tmpName.text = antique.Name;
            m_tmpDescription.text = antique.Description;
            m_imgImage.SetSprite(antique.ImageAddress);
            m_tmpText.gameObject.SetActive(false);
            SetButtonsInteractable(true);
            RefreshHud();
        }

        private void ShowJudgment(JudgmentResult result)
        {
            string prefix = result.IsCorrect ? "鉴定正确！" : "鉴定失误！";
            string score = result.IsCorrect ? $" +{result.ScoreDelta}" : " 信誉 -1";
            m_tmpText.text = $"{prefix}{score}\n{result.FeedbackLine}";
            m_tmpText.gameObject.SetActive(true);
            ContinueAfterFeedbackAsync().Forget();
            RefreshHud();
        }

        private async UniTaskVoid ContinueAfterFeedbackAsync()
        {
            await UniTask.Delay(850, ignoreTimeScale: true);
            if (!_isAlive || _session == null || _session.State != RoundState.Feedback) return;
            _session.ContinueAfterFeedback();
        }

        private void ShowResult(SessionResult result)
        {
            if (!_isAlive) return;
            GameModule.UI.ShowUIAsync<ResultUI>(result);
            Close();
        }

        private void RefreshHud()
        {
            if (_session == null) return;
            m_tmpLevel.text = _session.Level.Name;
            m_tmpTimer.text = $"{Mathf.CeilToInt(_session.RemainingSeconds):00}s";
            m_tmpScore.text = $"分数 {_session.Score}";
            m_tmpReputation.text = $"信誉 {new string('♥', _session.Reputation)}";
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
            session.AntiquePresented -= PresentAntique;
            session.Judged -= ShowJudgment;
            session.Finished -= ShowResult;
        }
    }
}

namespace GameLogic
{
    /// <summary>
    /// 单局结算与重开入口；UI 绑定由 ResultUI_Gen.g.cs 自动生成。
    /// 由 SortingDeskUI 在 GameSessionManager.Finished 回调中打开，UserData 为 SessionResult。
    /// </summary>
    [Window(UILayer.UI, location: "ResultUI", fullScreen: true)]
    public partial class ResultUI
    {
        private SessionResult _result;

        private partial void OnClickAgainBtn()
        {
            // 重开当前关，而不是固定重开第一关。
            GameModule.UI.ShowUIAsync<SortingDeskUI>(DemoAntiqueCatalog.LoadLevel(_result.LevelId));
            Close();
        }

        private partial void OnClickBackBtn()
        {
            // 返回主菜单，不保留上一局 Session。
            GameModule.UI.ShowUIAsync<MainMenuUI>();
            Close();
        }

        private partial void OnClickNextLevelBtn()
        {
            // 只有分数达到 level.passScore 且配置了 unlockLevelId 时才允许进入下一关。
            if (!_result.CanEnterNextLevel) return;
            GameModule.UI.ShowUIAsync<SortingDeskUI>(DemoAntiqueCatalog.LoadLevel(_result.NextLevelId));
            Close();
        }

        protected override void OnRefresh()
        {
            // ShowUIAsync<ResultUI>(result) 的 result 会由 UI 框架放入 UserData。
            _result = UserData is SessionResult result ? result : default;
            m_tmpTitle.text = _result.ReputationExhausted
                ? "信誉归零，老板把你请出去了"
                : _result.IsPassed ? "挑战成功！" : "分数不足，挑战失败";
            m_tmpScore.text = $"{_result.LevelName}\n得分：{_result.Score}";
            m_tmpAccuracy.text = $"正确率：{_result.AccuracyPercent}%（{_result.CorrectCount}/{_result.JudgedCount}）";
            m_tmpCombo.text = $"最高连击：x{_result.MaxCombo}";

            // 未达通关分、信誉归零、或本关没有下一关配置时，均不显示下一关按钮。
            m_btnNextLevel.gameObject.SetActive(_result.CanEnterNextLevel);
        }
    }
}

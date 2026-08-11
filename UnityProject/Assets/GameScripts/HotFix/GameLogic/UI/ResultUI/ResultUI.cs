namespace GameLogic
{
    /// <summary>单局结算与重开入口；UI 绑定由 ResultUI_Gen.g.cs 自动生成。</summary>
    [Window(UILayer.UI, location: "ResultUI", fullScreen: true)]
    public partial class ResultUI
    {
        private SessionResult _result;

        private partial void OnClickAgainBtn()
        {
            GameModule.UI.ShowUIAsync<SortingDeskUI>(DemoAntiqueCatalog.LoadLevelOne());
            Close();
        }

        private partial void OnClickBackBtn()
        {
            GameModule.UI.ShowUIAsync<MainMenuUI>();
            Close();
        }

        protected override void OnRefresh()
        {
            _result = UserData is SessionResult result ? result : default;
            m_tmpTitle.text = _result.ReputationExhausted ? "信誉归零，老板把你请出去了" : "本局鉴定完成！";
            m_tmpScore.text = $"{_result.LevelName}\n得分：{_result.Score}";
            m_tmpAccuracy.text = $"正确率：{_result.AccuracyPercent}%（{_result.CorrectCount}/{_result.JudgedCount}）";
            m_tmpCombo.text = $"最高连击：x{_result.MaxCombo}";
        }
    }
}

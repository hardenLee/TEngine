using UnityEngine;
using UnityEngine.UI;

namespace Launcher
{
    /// <summary>
    /// 启动闪屏界面。预制体：<c>Launcher/Resources/UIWindow/SplashScreenUI</c>。
    /// </summary>
    public class SplashScreenUI : UIBase
    {
        #region 脚本工具生成的代码

        private Image m_imgBg;
        private Text m_textVersion;

        protected override void ScriptGenerator()
        {
            m_imgBg = FindChildComponent<Image>("m_imgBg");
            m_textVersion = FindChildComponent<Text>("m_textVersion");
        }

        #endregion

        protected override bool FullScreen => true;

        public override void OnInit(object param)
        {
            base.OnInit(param);

            if (rectTransform != null)
            {
                rectTransform.SetAsLastSibling();
            }

            if (m_textVersion != null && param is string versionText)
            {
                m_textVersion.text = $"我是闪屏: {versionText}";
            }
        }
    }
}

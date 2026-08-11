using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameConfig.item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.SceneManagement;

namespace GameLogic
{
    /// <summary>Demo 主菜单；第一关配置准备好后从此处进入工作台。</summary>
    [Window(UILayer.UI, location: "MainMenuUI", fullScreen: true)]
    public partial class MainMenuUI
    {
        private partial void OnClickStartButtonBtn()
        {
            GameModule.UI.ShowUIAsync<SortingDeskUI>(DemoAntiqueCatalog.LoadLevelOne());
            Close();
        }

        private partial void OnClickQuitButtonBtn()
        {
            // 编辑器中不退出；真机和桌面包均可正确结束应用。
            UnityEngine.Application.Quit();
        }


    }
}

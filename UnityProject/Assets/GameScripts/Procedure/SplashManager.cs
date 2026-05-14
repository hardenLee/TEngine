using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Procedure
{
    /// <summary>
    /// 启动闪屏：可选加载 Resources 下预制体，展示固定时长后结束。
    /// 预制路径与 Launcher 一致：<c>Resources/UIWindow/SplashScreen</c>（无则仅等待时长）。
    /// </summary>
    public static class SplashManager
    {
        private const string SplashPrefabResourcesPath = "UIWindow/SplashScreenUI";
        private const string UiCanvasHierarchyPath = "UIRoot/UICanvas";

        /// <summary>
        /// 播放闪屏并等待结束（时长到达后销毁实例）。
        /// </summary>
        /// <param name="splashTimeSeconds">展示时长（秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async UniTask RunAsync(float splashTimeSeconds, CancellationToken cancellationToken = default)
        {
            if (splashTimeSeconds < 0f)
            {
                splashTimeSeconds = 0f;
            }

            GameObject instance = null;
            var parent = GameObject.Find(UiCanvasHierarchyPath)?.transform;

            try
            {
                var prefab = Resources.Load<GameObject>(SplashPrefabResourcesPath);
                if (prefab != null && parent != null)
                {
                    instance = Object.Instantiate(prefab, parent, false);
                    instance.name = nameof(SplashManager) + "_Instance";

                    if (instance.TryGetComponent<RectTransform>(out var rt))
                    {
                        StretchFullScreen(rt, parent as RectTransform);
                    }
                }
                else if (prefab == null)
                {
                    Log.Info($"[SplashManager] 未找到 Resources/{SplashPrefabResourcesPath}，仅等待 {splashTimeSeconds}s。");
                }
                else
                {
                    Log.Warning($"[SplashManager] 未找到 {UiCanvasHierarchyPath}，无法挂载闪屏预制。");
                }

                if (splashTimeSeconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(splashTimeSeconds), cancellationToken: cancellationToken);
                }
            }
            finally
            {
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }
        }

        private static void StretchFullScreen(RectTransform rt, RectTransform parentRect)
        {
            if (parentRect == null)
            {
                return;
            }

            rt.SetParent(parentRect, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.SetAsLastSibling();
        }
    }
}

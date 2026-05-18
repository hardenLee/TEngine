using System;
using Cysharp.Threading.Tasks;
using Launcher;
using TEngine;
using UnityEngine;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureModule>;

namespace Procedure
{
    /// <summary>
    /// 流程 => 闪屏。
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        private const float SplashDurationSeconds = 3f;

        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            _procedureOwner = procedureOwner;
            Log.Info("ProcedureSplash: OnEnter, show SplashScreenUI.");
            RunSplashThenInitPackage().Forget();
        }

        private async UniTaskVoid RunSplashThenInitPackage()
        {
            try
            {
                if (!LauncherMgr.ShowSplash(Application.version))
                {
                    Log.Info("ProcedureSplash: SplashScreenUI prefab not found, skip splash delay.");
                }
                else if (SplashDurationSeconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(SplashDurationSeconds));
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("ProcedureSplash: splash cancelled.");
            }
            catch (Exception e)
            {
                Log.Error($"ProcedureSplash: splash error: {e}");
            }
            finally
            {
                LauncherMgr.CloseSplash();
            }

            if (_procedureOwner != null)
            {
                ChangeState<ProcedureInitPackage>(_procedureOwner);
            }
        }
    }
}

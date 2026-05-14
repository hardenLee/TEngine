using Cysharp.Threading.Tasks;
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
            Log.Info("ProcedureSplash: OnEnter, start SplashManager.");
            RunSplashThenInitPackage().Forget();
        }

        private async UniTaskVoid RunSplashThenInitPackage()
        {
            try
            {
                await SplashManager.RunAsync(SplashDurationSeconds);
            }
            catch (System.OperationCanceledException)
            {
                Log.Info("ProcedureSplash: splash cancelled.");
            }
            catch (System.Exception e)
            {
                Log.Error($"ProcedureSplash: splash error: {e}");
            }

            if (_procedureOwner != null)
            {
                ChangeState<ProcedureInitPackage>(_procedureOwner);
            }
        }
    }
}

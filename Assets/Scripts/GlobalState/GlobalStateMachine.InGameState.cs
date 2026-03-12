using GonDraz.Events;
using GonDraz.UI.Route;
using UI.Screens;

namespace GlobalState
{
    public partial class GlobalStateMachine
    {
        public class InGameState : BaseGlobalState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                BaseEventManager.GamePause += OnGamePause;
                BaseEventManager.ApplicationPause += OnApplicationPause;

                RouteManager.Go(typeof(InGameScreen)
                );
            }

            public override void OnExit()
            {
                base.OnExit();
                BaseEventManager.GamePause -= OnGamePause;
                BaseEventManager.ApplicationPause -= OnApplicationPause;
            }

            private void OnApplicationPause(bool obj)
            {
                Host.ChangeState<InGamePauseState>();
            }

            private void OnGamePause()
            {
                Host.ChangeState<InGamePauseState>();
            }
        }
    }
}
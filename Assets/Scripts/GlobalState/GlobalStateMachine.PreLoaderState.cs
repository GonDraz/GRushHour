using GonDraz.Events;
using Managers;

namespace GlobalState
{
    public partial class GlobalStateMachine
    {
        private class PreLoaderState : BaseGlobalState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                BaseEventManager.ApplicationLoadFinished += ApplicationLoadFinished;
                // RouteManager.Go(typeof(PreLoaderScreen));
            }


            public override void OnExit()
            {
                base.OnExit();
                BaseEventManager.ApplicationLoadFinished -= ApplicationLoadFinished;
            }

            private void ApplicationLoadFinished()
            {
                if (ScoreManager.HighScore.Value == 0)
                {
                    Host.ChangeState<InGameState>();
                }
                else
                {
                    Host.ChangeState<MenuState>();
                }
            }
        }
    }
}
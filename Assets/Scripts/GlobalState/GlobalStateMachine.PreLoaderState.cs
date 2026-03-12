using GonDraz.Events;

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
                Host.ChangeState<MenuState>();
            }
        }
    }
}
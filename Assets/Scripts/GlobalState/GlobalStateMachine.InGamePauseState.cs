using GonDraz.UI.Route;
using UI.Screens;

namespace GlobalState
{
    public partial class GlobalStateMachine
    {
        public class InGamePauseState : BaseGlobalState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                RouteManager.Go(typeof(InGamePauseScreen));
            }
        }
    }
}
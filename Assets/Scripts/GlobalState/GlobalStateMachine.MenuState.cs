using GonDraz.UI.Route;
using UI.Screens;

namespace GlobalState
{
    public partial class GlobalStateMachine
    {
        public class MenuState : BaseGlobalState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                RouteManager.Go(typeof(MenuScreen));
            }
        }
    }
}
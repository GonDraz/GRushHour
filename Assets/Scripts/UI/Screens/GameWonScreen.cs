using System;
using GlobalState;
using GonDraz.UI;

namespace UI.Screens
{
    public class GameWonScreen : Presentation
    {
        public override void Show(Action callback = null)
        {
            base.Show(callback);
        }

        public void OnHomeButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.MenuState>(false);
        }

        public void OnNextLevelButtonClick()
        {
        }
    }
}
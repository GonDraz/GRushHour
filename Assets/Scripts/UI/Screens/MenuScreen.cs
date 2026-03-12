using GonDraz.UI;
using GonDraz.UI.Route;
using Managers;
using UI.Popups;

namespace UI.Screens
{
    public class MenuScreen : Presentation
    {
        public void OnLevelButtonClick()
        {
            RouteManager.Push(typeof(LevelSelectionPopup));
        }
    }
}
using GonDraz.UI;
using Managers;

namespace UI.Screens
{
    public class GameWonScreen : Presentation
    {
        public void OnNextLevelButtonClick()
        {
            // Tiến level tiếp theo và bắt đầu chơi
            // Advance to the next level and start gameplay.
            LevelManager.PlayNextLevel();
        }
    }
}
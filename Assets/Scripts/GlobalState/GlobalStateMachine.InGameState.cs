using GonDraz.Events;
using GonDraz.UI.Route;
using Managers;
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
                // Mirror Exit.cs: listen for target block reaching exit → go to GameWon
                EventManager.PuzzleSolved += OnPuzzleSolved;

                RouteManager.Go(typeof(InGameScreen));
            }

            public override void OnExit()
            {
                base.OnExit();
                BaseEventManager.GamePause -= OnGamePause;
                BaseEventManager.ApplicationPause -= OnApplicationPause;
                EventManager.PuzzleSolved -= OnPuzzleSolved;
            }

            private void OnApplicationPause(bool obj)
            {
                Host.ChangeState<InGamePauseState>();
            }

            private void OnGamePause()
            {
                Host.ChangeState<InGamePauseState>();
            }

            // Mirrors Exit.cs LoadLevel: target block exits board → advance to GameWon state
            private void OnPuzzleSolved()
            {
                Host.ChangeState<GameWon>();
            }
        }
    }
}
using System;
using GlobalState;
using GonDraz.UI;
using Managers;
using Tutorial;
using UnityEngine;

namespace UI.Screens
{
    public class InGamePauseScreen : Presentation
    {
        // Resume — return to game without resetting the puzzle
        public void OnBackGameButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false);
        }

        // Restart — reset puzzle board then re-enter InGameState
        public void OnRestartButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false);
            EventManager.SetupGameplay.Invoke();
        }
    }
}
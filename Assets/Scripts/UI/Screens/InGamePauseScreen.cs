using System;
using GlobalState;
using GonDraz.Events;
using GonDraz.UI;
using Tutorial;
using UnityEngine;

namespace UI.Screens
{
    public class InGamePauseScreen : Presentation
    {
        [SerializeField] private GameObject homeButton;

        public override void Show(Action callback = null)
        {
            base.Show(callback);
            homeButton.SetActive(TutorialManager.IsCompleted(TutorialType.GameplayIntro));
        }

        public void OnBackGameButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false);
        }

        public void OnHomeButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.MenuState>(false);
        }

        public void OnRestartButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false);
            // EventManager.SetupGamePlay?.Invoke();
        }
    }
}
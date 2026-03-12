using System;
using GlobalState;
using GonDraz.UI;

namespace UI.Screens
{
    public class InGameScreen : Presentation
    {
        public static InGameScreen Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public override void Show(Action callback = null)
        {
            base.Show(callback + OnScreenSizeChanged);
        }

        public override void Subscribe()
        {
            base.Subscribe();
            // EventManager.ScreenSizeChanged += OnScreenSizeChanged;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            // EventManager.ScreenSizeChanged -= OnScreenSizeChanged;
        }

        private void OnScreenSizeChanged()
        {
            // topBackground.anchoredPosition = PerfectCamera.TopBar;
            // topBar.anchoredPosition = PerfectCamera.TopBar;
        }


        public void OnPauseButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGamePauseState>();
        }
    }
}
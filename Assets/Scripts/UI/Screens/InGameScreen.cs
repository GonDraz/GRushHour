using System;
using GlobalState;
using GonDraz.UI;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.Screens
{
    public class InGameScreen : Presentation
    {
        [SerializeField] private TMP_Text autoSolveButtonLabel;

        // Text khi chưa chạy / đang chạy
        // Label text for idle / running states
        private const string LabelIdle    = "Auto Solve";
        private const string LabelRunning = "Stop";

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
            EventManager.AutoSolvingStateChanged += OnAutoSolvingStateChanged;
            // EventManager.ScreenSizeChanged += OnScreenSizeChanged;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            EventManager.AutoSolvingStateChanged -= OnAutoSolvingStateChanged;
            // EventManager.ScreenSizeChanged -= OnScreenSizeChanged;
        }

        // Cập nhật label nút khi trạng thái auto-solve thay đổi
        // Update button label when auto-solve state changes
        private void OnAutoSolvingStateChanged(bool isRunning)
        {
            if (autoSolveButtonLabel != null)
                autoSolveButtonLabel.text = isRunning ? LabelRunning : LabelIdle;
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

        /// <summary>
        ///     Gọi từ nút Auto Solve trên màn hình game.
        ///     Toggle auto-solve: bắt đầu nếu chưa chạy, dừng nếu đang chạy.
        ///     (Called by the Auto Solve button. Toggles auto-solve on/off.)
        /// </summary>
        public void OnAutoSolveButtonClick()
        {
            EventManager.AutoSolveToggle.Invoke();
        }
    }
}
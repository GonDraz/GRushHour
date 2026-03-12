using System;
using GlobalState;
using GonDraz.Events;
using GonDraz.UI;
using Managers;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace UI.Screens
{
    public class InGameScreen : Presentation
    {
        [SerializeField] private RectTransform topBackground;
        [SerializeField] private RectTransform topBar;

        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text streakText;
        [SerializeField] private GameObject boostersParent;
        [SerializeField] private GameObject fireBoosterButton;

        public GameObject BoostersParent => boostersParent;

        public static InGameScreen Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            fireBoosterButton.SetActive(false);
        }

        public override void Show(Action callback = null)
        {
            base.Show(callback + OnScreenSizeChanged);
        }

        public override void Subscribe()
        {
            base.Subscribe();
            ScoreManager.Score.OnChanged += ScoreOnChanged;
            // EventManager.ScreenSizeChanged += OnScreenSizeChanged;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            ScoreManager.Score.OnChanged -= ScoreOnChanged;
            // EventManager.ScreenSizeChanged -= OnScreenSizeChanged;
        }

        private void OnScreenSizeChanged()
        {
            // topBackground.anchoredPosition = PerfectCamera.TopBar;
            // topBar.anchoredPosition = PerfectCamera.TopBar;
        }


        private void ScoreOnChanged(int pre, int cur)
        {
            Tween.Custom(pre, cur, 0.375f,
                    newVal => scoreText.text = newVal.ToString("N0"))
                .OnComplete(() => scoreText.text = cur.ToString("N0"));
        }

        public void OnPauseButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGamePauseState>();
        }

        public void FireBoosterButtonClick()
        {
            fireBoosterButton.SetActive(false);
        }
    }
}
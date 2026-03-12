using System;
using Ami.BroAudio;
using GlobalState;
using GonDraz.UI;
using Managers;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace UI.Screens
{
    public class GameOverScreen : Presentation
    {
        [SerializeField] private SoundID gameOverSound;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject highScoreText;

        public override void Show(Action callback = null)
        {
            base.Show(callback);

            BroAudio.Play(gameOverSound);

            if (ScoreManager.Score.Value == ScoreManager.HighScore.Value)
            {
                highScoreText.transform.localScale = Vector3.zero;
                highScoreText.SetActive(true);
                scoreText.text = $"{ScoreManager.Score}";
                Tween.Custom(0, ScoreManager.Score.Value, 0.375f,
                        newVal => scoreText.text = newVal.ToString("N0"))
                    .OnComplete(() => scoreText.text = ScoreManager.Score.Value.ToString("N0"));
                Tween.Scale(highScoreText.transform, Vector3.one, 0.25f);
            }
            else
            {
                highScoreText.SetActive(false);
                scoreText.text = $"Score: {ScoreManager.Score}";
                Tween.Custom(0, ScoreManager.Score.Value, 0.375f,
                        newVal => scoreText.text = "Score: " + newVal.ToString("N0"))
                    .OnComplete(() => scoreText.text = "Score: " + ScoreManager.Score.Value.ToString("N0"));
            }
        }


        public void OnRestartButtonClick()
        {
            GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false);
            EventManager.SetupGameplay.Invoke();
        }
    }
}
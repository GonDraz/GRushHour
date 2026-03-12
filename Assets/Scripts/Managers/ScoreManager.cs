using GonDraz.Events;
using GonDraz.Observable;
using GonDraz.PlayerPrefs;

namespace Managers
{
    public static class ScoreManager
    {
        public static readonly GObservableValue<int> Score = new(0, new GEvent<int, int>(
            "ScoreChanged", (_, curValue) =>
            {
                if (Score.Value > HighScore.Value) HighScore.Value = curValue;
            }));

        // HighScore is persisted automatically to PlayerPrefs
        public static readonly GObservableValue<int> HighScore =
            GObservablePlayerPrefs.CreateObservableInt("HighScore");

        public static readonly GObservableValue<int> PreviousHighScore =
            GObservablePlayerPrefs.CreateObservableInt("PreviousHighScore");

        static ScoreManager()
        {
            // BaseEventManager.SetupGamePlay += OnSetupGamePlay;
        }

        private static void OnSetupGamePlay()
        {
            ResetScore();
        }

        private static void ResetScore()
        {
            Score.Value = 0;
            if (HighScore.Value >= PreviousHighScore.Value) PreviousHighScore.Value = HighScore.Value;
        }

        public static string ToShortFormat(this int value)
        {
            return value switch
            {
                < 1000 => value.ToString(),
                < 1000000 => (value / 1000f).ToString("0.#") + "K",
                < 1000000000 => (value / 1000000f).ToString("0.#") + "M",
                _ => (value / 1000000000f).ToString("0.#") + "B"
            };
        }
    }
}
using System;
using Cysharp.Threading.Tasks;
using GonDraz.Events;
using UnityEngine;

namespace Managers
{
    public static class CountdownManager
    {
        public static GEvent Tick = new("Countdown-Tick");


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnInitializeBeforeSceneLoad()
        {
            StartCountDown();
        }

        private static async void StartCountDown()
        {
            while (true)
            {
                Tick?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
using System;
using Ami.BroAudio;
using GonDraz.Events;
using GonDraz.Observable;
using GonDraz.PlayerPrefs;
using UnityEngine;

namespace Managers
{
    public static class VolumeManager
    {
        public static GObservableDictionary<BroAudioType, float> Volumes { get; } = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            InitializeVolumes();
            Subscribe();
        }

        private static void InitializeVolumes()
        {
            foreach (BroAudioType type in Enum.GetValues(typeof(BroAudioType)))
            {
                if (type == BroAudioType.None || type == BroAudioType.All)
                    continue;

                SetVolumeFromPrefs(type, 1f);
            }

            Volumes.OnItemAdded = new GEvent<BroAudioType, float>(SaveVolume);
            Volumes.OnItemUpdated = new GEvent<BroAudioType, float>(SaveVolume);
        }

        private static void SetVolumeFromPrefs(BroAudioType type, float defaultValue)
        {
            var key = GetVolumeKey(type);
            var savedVolume = GPlayerPrefs.GetFloat(key, defaultValue);
            Volumes[type] = savedVolume;
        }

        private static void SaveVolume(BroAudioType type, float volume)
        {
            var key = GetVolumeKey(type);
            GPlayerPrefs.SetFloat(key, volume);
            BroAudio.SetVolume(type, volume);
        }

        private static string GetVolumeKey(BroAudioType type)
        {
            return $"{type}Volume";
        }

        private static void Subscribe()
        {
            BaseEventManager.ApplicationLoadFinished += OnApplicationLoadFinished;
        }

        private static void OnApplicationLoadFinished()
        {
            foreach (var kvp in Volumes.Dictionary) BroAudio.SetVolume(kvp.Key, kvp.Value);
        }

        public static void SetVolume(BroAudioType type, float volume)
        {
            Volumes[type] = Mathf.Clamp01(volume);
        }

        public static float GetVolume(BroAudioType type)
        {
            return Volumes.TryGetValue(type, out var volume) ? volume : 1f;
        }
    }
}
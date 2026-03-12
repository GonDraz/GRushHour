using System;
using Ami.BroAudio;
using GonDraz.UI;
using UnityEngine;

namespace UI.Screens
{
    public class SettingsScreen : Presentation
    {
        [Header("Volume Sliders")] [SerializeField]
        private VolumeSlider volumeSliderPrefab;

        [SerializeField] private Transform volumeSlidersContainer;

        private void Start()
        {
            CreateVolumeSliders();
        }

        private void CreateVolumeSliders()
        {
            if (volumeSliderPrefab == null || volumeSlidersContainer == null)
            {
                Debug.LogWarning("VolumeSlider prefab or container is not assigned!");
                return;
            }

            // Tạo slider cho mỗi BroAudioType
            foreach (BroAudioType type in Enum.GetValues(typeof(BroAudioType)))
            {
                if (type == BroAudioType.None || type == BroAudioType.All)
                    continue;

                CreateSliderForType(type);
            }
        }

        private void CreateSliderForType(BroAudioType type)
        {
            var sliderInstance = Instantiate(volumeSliderPrefab, volumeSlidersContainer);
            sliderInstance.name = $"{type}VolumeSlider";
            sliderInstance.SetAudioType(type);
        }
    }
}
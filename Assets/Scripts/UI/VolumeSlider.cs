using Ami.BroAudio;
using GonDraz.Base;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class VolumeSlider : BaseBehaviour
    {
        [SerializeField] private BroAudioType audioType;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text valueText;

        [SerializeField] private Image icon;
        [SerializeField] private Sprite soundIcon;
        [SerializeField] private Sprite soundMuteIcon;

        private void Start()
        {
            InitializeSlider();
        }

        public void SetAudioType(BroAudioType type)
        {
            audioType = type;
            InitializeSlider();
        }

        public override bool SubscribeUsingOnEnable()
        {
            return true;
        }

        public override void Subscribe()
        {
            base.Subscribe();

            if (slider) slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();

            if (slider) slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void InitializeSlider()
        {
            if (!slider) return;

            // Set slider range
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Load current volume
            var currentVolume = VolumeManager.GetVolume(audioType);
            slider.SetValueWithoutNotify(currentVolume);

            // Update UI
            UpdateValueText(currentVolume);
            UpdateIcon(currentVolume);
        }

        private void OnSliderValueChanged(float value)
        {
            VolumeManager.SetVolume(audioType, value);
            UpdateValueText(value);
            UpdateIcon(value);
        }


        private void UpdateValueText(float value)
        {
            if (valueText) valueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void UpdateIcon(float value)
        {
            icon.sprite = value > 0f ? soundIcon : soundMuteIcon;
        }
    }
}
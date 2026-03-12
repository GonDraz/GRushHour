using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AnimImageFlashEffect : MonoBehaviour
    {
        [SerializeField] private Image effectImages;
        [SerializeField] private Color startColor = new(1f, 1f, 1f, 0f);
        [SerializeField] private Color midColor = new(1f, 1f, 1f, 0.25f);
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.125f;
        [SerializeField] private float delayBetweenPulses = 0.25f;
        [SerializeField] private float delayAtEnd = 1f;

        private Sequence _boxEffectSequence;

        private void OnEnable()
        {
            _boxEffectSequence.Stop();
            _boxEffectSequence = Sequence.Create(-1)
                .Chain(Tween.Custom(this, startColor, midColor, fadeInDuration,
                    (target, newVal) => target.SetAlphaBoxEffect(newVal), Ease.InOutSine))
                .Chain(Tween.Custom(this, midColor, startColor, fadeOutDuration,
                    (target, newVal) => target.SetAlphaBoxEffect(newVal), Ease.InOutSine))
                .Chain(Tween.Delay(delayBetweenPulses))
                .Chain(Tween.Custom(this, startColor, midColor, fadeInDuration,
                    (target, newVal) => target.SetAlphaBoxEffect(newVal), Ease.InOutSine))
                .Chain(Tween.Custom(this, midColor, startColor, fadeOutDuration,
                    (target, newVal) => target.SetAlphaBoxEffect(newVal), Ease.InOutSine))
                .Chain(Tween.Delay(delayAtEnd));
        }

        private void OnDisable()
        {
            _boxEffectSequence.Stop();
            SetAlphaBoxEffect(startColor);
        }

        private void SetAlphaBoxEffect(Color color)
        {
            effectImages.color = color;
        }
    }
}
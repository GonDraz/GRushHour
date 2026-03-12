using PrimeTween;
using UnityEngine;

namespace UI
{
    public class AnimZoomButton : MonoBehaviour
    {
        [SerializeField] private GameObject bgButton;
        [SerializeField] private float zoomedInScale = 1.2f;
        [SerializeField] private float zoomedOutScale = 1f;
        [SerializeField] private float zoomDuration = 1f;

        private Sequence _buttonSequence;
        private bool _isPaused;

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            Pause();
            bgButton.transform.localScale = Vector3.one * zoomedOutScale;
        }

        public void Pause()
        {
            if (!_isPaused && _buttonSequence.isAlive)
            {
                _buttonSequence.Stop();
                _isPaused = true;
            }
        }

        public void Play()
        {
            if (_isPaused)
            {
                _buttonSequence.Stop();

                _buttonSequence = Sequence.Create(-1)
                    .Chain(Tween.Scale(bgButton.transform, zoomedInScale, zoomDuration))
                    .Chain(Tween.Delay(zoomDuration))
                    .Chain(Tween.Scale(bgButton.transform, zoomedOutScale, zoomDuration));
                _isPaused = false;
            }
        }
    }
}
using Ami.BroAudio;
using GonDraz.Base;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GonDraz.UI.Selectable
{
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.UI.Selectable), typeof(CanvasGroup), typeof(RectTransform))]
    public class SelectableFeel : BaseBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private SelectableFeelConfig config;

        [SerializeField] private bool setDefault = true;
        private float _alphaDefault = -1f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector3 _rotationDefault;

        private Vector3 _scaleDefault;

        private Sequence _sequence;
        private bool _setupDefaultTransformCompleted;

        private RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetUpDefaultValueTransform();
            if (setDefault) SetDefaultTransform();
        }

        protected override void OnDisable()
        {
            _sequence.Stop();
            base.OnDisable();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
#endif

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!config) return;

            var cfg = config.pointerDown;
            if(config.buttonClickSound.IsValid()) config.buttonClickSound.Play();
            PlaySequence(cfg);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!config) return;

            var cfg = config.pointerUp;

            _sequence.Stop();

            _sequence = Sequence.Create();

            if (setDefault && _setupDefaultTransformCompleted)
            {
                cfg.rotation = _rotationDefault;
                cfg.scale = _scaleDefault;
                cfg.alpha = _alphaDefault;
            }

            PlaySequence(cfg);
        }

        private void SetUpDefaultValueTransform()
        {
            if (_setupDefaultTransformCompleted) return;
            _setupDefaultTransformCompleted = true;
            _rotationDefault = transform.localEulerAngles;
            _scaleDefault = transform.localScale;
            _alphaDefault = CanvasGroup.alpha;
        }

        private void PlaySequence(SelectableFeelConfig.Effect cfg)
        {
            _sequence.Stop();
            _sequence = Sequence.Create();
            if (transform.localRotation != Quaternion.Euler(cfg.rotation))
                _sequence.Group(Tween.LocalRotation(transform, cfg.rotation, cfg.duration, cfg.ease));
            if (transform.localScale != cfg.scale)
                _sequence.Group(Tween.Scale(transform, cfg.scale, cfg.duration, cfg.ease));
            if (!Mathf.Approximately(CanvasGroup.alpha, cfg.alpha))
                _sequence.Group(Tween.Alpha(CanvasGroup, cfg.alpha, cfg.duration, cfg.ease));
        }

        private void SetDefaultTransform()
        {
            if (!_setupDefaultTransformCompleted) return;
            transform.localEulerAngles = _rotationDefault;
            transform.localScale = _scaleDefault;
            CanvasGroup.alpha = _alphaDefault;
        }
    }
}
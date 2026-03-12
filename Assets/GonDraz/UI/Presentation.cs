using System;
using GonDraz.Base;
using GonDraz.Extensions;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GonDraz.UI
{
    [DisallowMultipleComponent]
    [SelectionBase]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Presentation : BaseBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] [Range(0, 4)] private float showDuration = 0.25f;
        [SerializeField] [Range(0, 4)] private float hideDuration = 0.125f;

        protected Sequence Sequence;
        private CanvasGroup CanvasGroup => canvasGroup ??= gameObject.GetOrAddComponent<CanvasGroup>();

        protected virtual void Awake()
        {
            ResetUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Sequence.Stop();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
#endif

        protected virtual void ResetUI()
        {
            CanvasGroup.alpha = 0f;
        }

        private void CreateSequence()
        {
            Sequence.Stop();
            Sequence = Sequence.Create();
        }

        public override bool SubscribeUsingOnEnable()
        {
            return true;
        }

        public override bool UnsubscribeUsingOnDisable()
        {
            return true;
        }

        [Button]
        public virtual void Show(Action callback = null)
        {
            Active();
            CreateSequence();
            CanvasGroup.blocksRaycasts = false; // Block all clicks during show
            ChangeAlphaCanvasGroup(1f, showDuration, () =>
            {
                CanvasGroup.blocksRaycasts = true; // Enable clicks after show
                callback?.Invoke();
            });
        }

        [Button]
        public virtual void Hide(Action callback = null)
        {
            CanvasGroup.blocksRaycasts = false; // Block all clicks during hide
            CreateSequence();
            ChangeAlphaCanvasGroup(0f, hideDuration, () =>
            {
                Inactive();
                callback?.Invoke();
            });
        }

        private void ChangeAlphaCanvasGroup(float to, float duration, Action callback = null)
        {
            if (duration == 0)
            {
                CanvasGroup.alpha = to;
                callback?.Invoke();
            }
            else
            {
                if (!Mathf.Approximately(CanvasGroup.alpha, to) && gameObject.activeInHierarchy)
                    Sequence.Chain(Tween.Alpha(CanvasGroup, to, duration));
                if (callback != null) Sequence.OnComplete(callback);
            }
        }
    }
}
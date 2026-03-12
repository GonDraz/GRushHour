using GonDraz.Extensions;
using UnityEngine;

namespace GonDraz.Effects
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public abstract class BaseUIEffect : BaseEffect
    {
        [Header("UI Effect Settings")] [SerializeField]
        protected CanvasGroup canvasGroup;

        [SerializeField] protected RectTransform targetRect;

        protected virtual void Awake()
        {
            if (!canvasGroup)
                canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            if (!targetRect)
                targetRect = gameObject.GetOrAddComponent<RectTransform>();
        }

        public override void OnGetFromPool()
        {
            base.OnGetFromPool();

            if (canvasGroup)
                canvasGroup.alpha = 1f;

            if (targetRect)
                targetRect.localScale = Vector3.one;
        }

        public RectTransform GetTargetRectTransform()
        {
            return targetRect;
        }
    }
}
using System;
using PrimeTween;
using UnityEngine;

namespace GonDraz.UI
{
    public abstract class Popup : Presentation
    {
        /// <summary>
        ///     Static callback invoked when a Popup's back button is clicked.
        ///     Set by the routing system (e.g., RouteManager) to handle navigation.
        /// </summary>
        public static Action OnBackButtonAction;

        [SerializeField] private GameObject contentRoot;
        [SerializeField] [Range(0, 4)] private float zoomShowDuration = 0.25f;
        [SerializeField] [Range(0, 4)] private float zoomHideDuration = 0.125f;

        private Sequence _scaleSequence;

        protected override void ResetUI()
        {
            base.ResetUI();

            if (contentRoot) contentRoot.transform.localScale = Vector3.zero;
        }

        public override void Show(Action callback = null)
        {
            ResetUI();
            base.Show(callback);

            ChangeScaleContentRoot(Vector3.one, zoomShowDuration);
        }

        public override void Hide(Action callback = null)
        {
            ChangeScaleContentRoot(Vector3.zero, zoomHideDuration);
            base.Hide(callback);
        }

        private void ChangeScaleContentRoot(Vector3 to, float duration, Action callback = null)
        {
            _scaleSequence.Complete();

            if (duration == 0)
            {
                contentRoot.transform.localScale = to;
                callback?.Invoke();
            }
            else
            {
                if (contentRoot.transform.localScale != to && gameObject.activeInHierarchy)
                {
                    _scaleSequence = Sequence.Create();
                    _scaleSequence.Group(Tween.Scale(contentRoot.transform, to, duration));

                    if (callback != null) _scaleSequence.OnComplete(callback);
                }
                else if (callback != null)
                {
                    callback?.Invoke();
                }
            }
        }

        public void OnBackButtonClicked()
        {
            OnBackButtonAction?.Invoke();
        }
    }
}
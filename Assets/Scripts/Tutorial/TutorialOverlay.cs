using GonDraz.UI.Ummask;
using PrimeTween;
using UnityEngine;

namespace Tutorial
{
    public class TutorialOverlay : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Unmask unmask;

        [SerializeField] private TutorialHand tutorialHand;

        // [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")] [SerializeField] private Vector2 defaultFocusPadding = new(20f, 20f);

        [Header("Fade Settings")] [SerializeField]
        private float fadeInDuration = 0.3f;

        [SerializeField] private float fadeOutDuration = 0.3f;

        private Tween _fadeTween;

        private void OnEnable()
        {
            TutorialManager.StepChanged += HandleStepChanged;
            TutorialManager.TutorialEnded += HandleTutorialEnded;

            // Start hidden; show only when a new tutorial step is triggered.
            HideOverlay();
        }

        private void OnDisable()
        {
            TutorialManager.StepChanged -= HandleStepChanged;
            TutorialManager.TutorialEnded -= HandleTutorialEnded;
        }

        private void HandleStepChanged(TutorialType type, TutorialStepData step)
        {
            ApplyStep(step);
        }

        private void HandleTutorialEnded(TutorialType type)
        {
            HideOverlay();
        }

        private void ApplyStep(TutorialStepData step)
        {
            if (step == null)
            {
                HideOverlay();
                return;
            }

            var target = TutorialManager.GetTarget(step.TargetId);

            if (target == null)
            {
                Debug.LogWarning(
                    $"[TutorialOverlay] Target '{step.TargetId}' not found! Make sure TutorialTarget component is attached and enabled.");
                HideOverlay();
                return;
            }

            ShowOverlay();

            // Sử dụng Unmask để highlight target
            if (unmask != null)
            {
                unmask.FitTarget = target;
                unmask.FitOnLateUpdate = true;

                // Apply padding nếu có
                var padding = step.FocusPadding != Vector2.zero ? step.FocusPadding : defaultFocusPadding;
                var unmaskRect = unmask.GetComponent<RectTransform>();
                if (unmaskRect != null) unmaskRect.sizeDelta = target.rect.size + padding * 2f;
            }

            // Hiển thị message
            // if (messageText != null)
            // {
            //     messageText.text = step.Message;
            // }

            // Đặt vị trí TutorialHand
            if (tutorialHand != null) tutorialHand.SetTarget(target);
        }

        private void ShowOverlay()
        {
            if (overlayRoot != null) overlayRoot.SetActive(true);

            // Re-enable Unmask component
            if (unmask != null && !unmask.enabled) unmask.enabled = true;

            // Fade in CanvasGroup
            FadeIn();
        }

        private void HideOverlay()
        {
            // Fade out trước, sau đó ẩn
            FadeOut();
        }

        /// <summary>
        ///     Fade in overlay
        /// </summary>
        private void FadeIn()
        {
            // Dừng tween cũ nếu có
            _fadeTween.Stop();

            // Thiết lập CanvasGroup nếu chưa có
            if (canvasGroup == null)
            {
                canvasGroup = overlayRoot?.GetComponent<CanvasGroup>();
                if (canvasGroup == null && overlayRoot != null) canvasGroup = overlayRoot.AddComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                _fadeTween = Tween.Alpha(canvasGroup, 1f, fadeInDuration, Ease.InOutQuad);
            }
        }

        /// <summary>
        ///     Fade out overlay
        /// </summary>
        private void FadeOut()
        {
            // Thiết lập CanvasGroup nếu chưa có
            if (canvasGroup == null)
            {
                canvasGroup = overlayRoot?.GetComponent<CanvasGroup>();
                if (canvasGroup == null && overlayRoot != null) canvasGroup = overlayRoot.AddComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                // Dừng tween cũ nếu có
                _fadeTween.Stop();

                _fadeTween = Tween.Alpha(canvasGroup, 0f, fadeOutDuration, Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        // Ẩn sau khi fade out xong
                        if (overlayRoot != null) overlayRoot.SetActive(false);

                        // Disable Unmask component
                        if (unmask != null)
                        {
                            unmask.enabled = false;
                            unmask.FitOnLateUpdate = false;
                        }
                    });
            }
            else
            {
                // Fallback nếu không có CanvasGroup
                if (overlayRoot != null) overlayRoot.SetActive(false);

                if (unmask != null)
                {
                    unmask.enabled = false;
                    unmask.FitOnLateUpdate = false;
                }
            }
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    /// <summary>
    ///     Component đính kèm vào UI element để đánh dấu nó là tutorial target
    ///     Tự động đăng ký/hủy đăng ký với TutorialManager
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TutorialTarget : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("ID duy nhất của target này (vd: 'play_button', 'power_up_button')")]
        [SerializeField]
        private string targetId;

        [Tooltip("Tự động gọi NextStep khi button này được click (trong tutorial)")] [SerializeField]
        private bool autoCompleteOnClick = true;

        [ShowIf("autoCompleteOnClick")] [SerializeField]
        private Button button;

        private Button _button;

        private RectTransform _rectTransform;

        public string TargetId => targetId;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _button = !button ? GetComponent<Button>() : button;
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(targetId))
            {
                Debug.LogWarning($"TutorialTarget on {gameObject.name} has empty targetId!", this);
                return;
            }

            // Đăng ký với TutorialManager
            TutorialManager.RegisterTarget(targetId, _rectTransform);

            // Thêm listener cho button nếu có
            if (autoCompleteOnClick && _button) _button.onClick.AddListener(OnTargetClicked);
        }

        private void OnDisable()
        {
            if (string.IsNullOrEmpty(targetId)) return;

            // Hủy đăng ký
            TutorialManager.RegisterTarget(targetId, null);

            // Xóa listener
            if (_button) _button.onClick.RemoveListener(OnTargetClicked);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-generate targetId từ GameObject name nếu trống
            if (string.IsNullOrEmpty(targetId)) targetId = gameObject.name.ToLower().Replace(" ", "_");
        }
#endif

        private void OnTargetClicked()
        {
            // Chỉ complete step nếu tutorial đang active và target này đang được focus
            if (TutorialManager.IsActive)
            {
                var currentStep = TutorialManager.GetCurrentStepData();
                if (currentStep != null && currentStep.TargetId == targetId) TutorialManager.NextStep();
            }
        }
    }
}
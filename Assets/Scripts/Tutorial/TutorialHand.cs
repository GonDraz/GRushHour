using GonDraz.Base;
using PrimeTween;
using UnityEngine;

namespace Tutorial
{
    public class TutorialHand : BaseBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform handTransform;
        [SerializeField] private RectTransform fingerTransform;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private float handMoveDistance = 20f;
        [SerializeField] private float fingerMoveDistance = 30f;
        [SerializeField] private ParticleSystem particle;

        [Header("Position Settings")]
        [Tooltip("Offset từ center của target (để điều chỉnh vị trí tay)")]
        [SerializeField]
        private Vector2 handOffset = new(0f, 0f);

        [Header("Rotation Settings")]
        [Tooltip("Góc offset để hand hơi nghiêng thay vì cắm thẳng (độ)")]
        [SerializeField]
        private float rotationOffset = 15f;

        private RectTransform _currentTarget;
        private Vector2 _fingerInitialPosition;
        private Sequence _fingerSequence;
        private Vector2 _handInitialPosition;

        private Sequence _handSequence;


        protected override void OnEnable()
        {
            base.OnEnable();

            // Dừng animation cũ nếu có
            _handSequence.Stop();
            _fingerSequence.Stop();

            // Lưu vị trí ban đầu
            _handInitialPosition = handTransform.anchoredPosition;
            _fingerInitialPosition = fingerTransform.anchoredPosition;

            StartHandAnimation();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Dừng animation khi disable
            _handSequence.Stop();
            _fingerSequence.Stop();

            particle.Stop();

            // Reset lại vị trí ban đầu
            handTransform.anchoredPosition = _handInitialPosition;
            fingerTransform.anchoredPosition = _fingerInitialPosition;
        }

        /// <summary>
        ///     Đặt target cho tutorial hand và tự động di chuyển đến vị trí target
        /// </summary>
        public void SetTarget(RectTransform target)
        {
            _currentTarget = target;

            if (target == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Chuyển đổi vị trí target sang local space của rectTransform parent
            var targetWorldPos = target.position;
            Vector2 localPos;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform,
                    RectTransformUtility.WorldToScreenPoint(null, targetWorldPos),
                    null,
                    out localPos))
                rectTransform.anchoredPosition = localPos + handOffset;
            else
                // Fallback: sử dụng vị trí trực tiếp
                rectTransform.position = targetWorldPos;

            // Tính toán và áp dụng rotation để tay hướng về target
            RotateHandTowardsTarget();
        }

        /// <summary>
        ///     Xoay tay để hướng về vị trí target
        /// </summary>
        private void RotateHandTowardsTarget()
        {
            // Lấy độ phân giải màn hình
            var screenResolution = new Vector2(Screen.width, Screen.height);

            // Tính toán center của màn hình
            var screenCenter = screenResolution * 0.5f;

            // Chuyển đổi target screen position
            var targetWorldPos = _currentTarget.position;
            var targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, targetWorldPos);

            // Tính vector hướng từ center màn hình đến target screen position
            var directionToTarget = targetScreenPos - screenCenter;

            // Nếu vector quá gần 0, không xoay
            if (directionToTarget.magnitude < 0.1f) return;

            // Tính góc rotation (atan2 trả về radian)
            // Tính từ vị trí center màn hình
            var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

            // Điều chỉnh góc: 0 độ = hướng phải, -90 độ = hướng lên
            // Vì hand mặc định hướng lên (90 độ), nên chúng ta cộng thêm 90 độ
            angle -= 90f;

            // Thêm góc offset để hand hơi nghiêng thay vì cắm thẳng
            angle += rotationOffset;

            // Áp dụng rotation cho hand
            rectTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void StartHandAnimation()
        {
            var handStartY = _handInitialPosition.y;
            var fingerStartY = _fingerInitialPosition.y;

            // Animation cho bàn tay - di chuyển nhẹ nhàng lên xuống
            _handSequence = Sequence.Create(-1)
                .Chain(Tween.UIAnchoredPositionY(handTransform, handStartY - handMoveDistance, moveDuration,
                    Ease.InOutSine))
                .Chain(Tween.UIAnchoredPositionY(handTransform, handStartY, moveDuration, Ease.InOutSine));

            // Animation cho ngón tay - di chuyển mạnh hơn
            _fingerSequence = Sequence.Create(-1)
                .Chain(Tween.UIAnchoredPositionY(fingerTransform, fingerStartY - fingerMoveDistance, moveDuration,
                    Ease.InOutSine))
                .Chain(Tween.UIAnchoredPositionY(fingerTransform, fingerStartY, moveDuration, Ease.InOutSine))
                .ChainCallback(() => { particle.Play(); });
        }
    }
}
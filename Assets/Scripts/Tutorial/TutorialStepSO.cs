using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Tutorial
{
    /// <summary>
    ///     ScriptableObject định nghĩa một bước tutorial
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Tutorial Step", order = 1)]
    public class TutorialStepSO : ScriptableObject
    {
        [Header("Step Info")]
        [Tooltip("ID của UI element cần highlight (vd: 'play_button')")]
#if ODIN_INSPECTOR
        [LabelText("Target ID")]
        [InfoBox("Target ID must match TutorialTarget on UI")]
#endif
        public string targetId;

        [Tooltip("Tin nhắn hướng dẫn hiển thị cho người chơi")]
        [TextArea(2, 4)]
#if ODIN_INSPECTOR
        [LabelText("Message")]
#endif
        public string message;

        [Tooltip("Padding xung quanh vùng highlight")]
#if ODIN_INSPECTOR
        [LabelText("Focus Padding")]
#endif
        public Vector2 focusPadding = Vector2.zero;

        [Tooltip("Có chặn input ngoài vùng highlight không")]
#if ODIN_INSPECTOR
        [LabelText("Block Input")]
#endif
        public bool blockInput = true;

        [Tooltip("Có hiển thị hand pointer không")]
#if ODIN_INSPECTOR
        [LabelText("Show Hand")]
#endif
        public bool showHand = true;

        [Tooltip("Offset vị trí hand so với target (để điều chỉnh vị trí tay chỉ)")]
#if ODIN_INSPECTOR
        [LabelText("Hand Offset")]
        [ShowIf(nameof(showHand))]
#endif
        public Vector2 handOffset = Vector2.zero;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    ///     ScriptableObject chứa một chuỗi các bước tutorial
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/Tutorial Sequence", order = 0)]
    public class TutorialSequenceSO : ScriptableObject
    {
        [Header("Sequence Info")] [Tooltip("Loại tutorial")]
        public TutorialType tutorialType;

        [Tooltip("Tên hiển thị của tutorial sequence")]
        public string displayName;

        [Header("Steps")] [Tooltip("Danh sách các bước tutorial theo thứ tự")]
        public List<TutorialStepSO> steps = new();

        /// <summary>
        ///     Lấy số lượng bước trong sequence
        /// </summary>
        public int StepCount => steps?.Count ?? 0;

        /// <summary>
        ///     Kiểm tra có bước nào trong sequence không
        /// </summary>
        public bool HasSteps => steps != null && steps.Count > 0;

        /// <summary>
        ///     Lấy bước tutorial tại index
        /// </summary>
        public TutorialStepSO GetStep(int index)
        {
            if (steps == null || index < 0 || index >= steps.Count) return null;

            return steps[index];
        }
    }
}
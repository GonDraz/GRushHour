using System.Collections.Generic;
using System.Linq;
using GonDraz.Base;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    ///     Central registry for all tutorial sequences.
    ///     Đặt component này trên một GameObject trong scene và kéo thả tutorial sequences vào.
    /// </summary>
    public class TutorialRegistry : BaseBehaviourSingleton<TutorialRegistry>
    {
        private static TutorialRegistry _instance;

        [Header("Tutorial Sequences")]
        [Tooltip("Danh sách tất cả tutorial sequences trong game - Kéo thả config vào đây")]
        [SerializeField]
        private List<TutorialSequenceSO> sequences = new();

        /// <summary>
        ///     Đếm số lượng sequences đã đăng ký
        /// </summary>
        public int SequenceCount => sequences?.Count ?? 0;


        /// <summary>
        ///     Lấy tất cả sequences đã đăng ký
        /// </summary>
        public List<TutorialSequenceSO> GetAllSequences()
        {
            return sequences ?? new List<TutorialSequenceSO>();
        }

        /// <summary>
        ///     Tìm sequence theo tutorial type
        /// </summary>
        public TutorialSequenceSO GetSequence(TutorialType type)
        {
            if (sequences == null) return null;

            foreach (var sequence in sequences)
                if (sequence != null && sequence.tutorialType == type)
                    return sequence;

            return null;
        }

        /// <summary>
        ///     Kiểm tra có sequence nào cho tutorial type không
        /// </summary>
        public bool HasSequence(TutorialType type)
        {
            return GetSequence(type) != null;
        }

        protected override bool IsDontDestroyOnLoad()
        {
            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Validate registry trong Editor
        /// </summary>
        public void Validate()
        {
            if (sequences == null)
            {
                sequences = new List<TutorialSequenceSO>();
                return;
            }

            // Remove null entries
            sequences.RemoveAll(s => s == null);

            // Check for duplicates
            var typesSeen = new HashSet<TutorialType>();
            var duplicates = (from sequence in sequences
                where !typesSeen.Add(sequence.tutorialType)
                select sequence.tutorialType).ToList();

            if (duplicates.Count > 0)
                Debug.LogWarning(
                    $"[TutorialRegistry] Tìm thấy duplicate tutorial types: {string.Join(", ", duplicates)}", this);
        }

        private void OnValidate()
        {
            Validate();
        }
#endif
    }
}
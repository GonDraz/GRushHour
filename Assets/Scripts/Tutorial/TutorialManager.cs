using System.Collections.Generic;
using GonDraz.Events;
using GonDraz.PlayerPrefs;
using UnityEngine;

namespace Tutorial
{
    public sealed class TutorialStepData
    {
        public TutorialStepData(int index, string id, string message, string targetId)
        {
            Index = index;
            Id = id;
            Message = message;
            TargetId = targetId;
            FocusPadding = Vector2.zero;
        }

        public int Index { get; }
        public string Id { get; }
        public string Message { get; }
        public string TargetId { get; }
        public Vector2 FocusPadding { get; set; }
        public bool BlockInput { get; set; }
    }

    public enum TutorialType
    {
        GameplayIntro = 0,
        AchievementsIntro = 1,
        PowerUpIntro = 2
    }

    public static class TutorialManager
    {
        private const string PrefKeyPrefix = "tutorial.progress.";

        private static readonly Dictionary<TutorialType, TutorialSequenceSO> SequencesByType = new();

        private static readonly Dictionary<string, RectTransform> TargetsById = new();

        private static bool _isInitialized;

        public static GEvent<TutorialType, TutorialStepData> StepChanged = new("TutorialManager.StepChanged");
        public static GEvent<TutorialType> TutorialEnded = new("TutorialManager.TutorialEnded");

        static TutorialManager()
        {
            Initialize();
        }

        public static bool IsActive { get; private set; }

        public static TutorialType ActiveType { get; private set; }

        public static int ActiveStepIndex { get; private set; }

        /// <summary>
        ///     Load TutorialRegistry từ scene
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized) return;

            SequencesByType.Clear();

            // Tìm TutorialRegistry trong scene
            var registry = TutorialRegistry.Instance;

            if (registry == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[TutorialManager] TutorialRegistry not found in scene! " +
                                 "Create a GameObject and add TutorialRegistry component, then drag tutorial sequences into it.");
#endif
                _isInitialized = true;
                return;
            }

            // Load tất cả sequences từ registry
            var sequences = registry.GetAllSequences();

            foreach (var sequence in sequences)
                if (sequence != null && sequence.HasSteps)
                    SequencesByType[sequence.tutorialType] = sequence;

            _isInitialized = true;

#if UNITY_EDITOR
            Debug.Log(
                $"[TutorialManager] Loaded {SequencesByType.Count} tutorial sequences'");
#endif
        }

        /// <summary>
        ///     Lấy reference đến TutorialRegistry (for editor tools)
        /// </summary>
        public static TutorialRegistry GetRegistry()
        {
            return TutorialRegistry.Instance;
        }

        /// <summary>
        ///     Reload sequences từ registry (useful sau khi thay đổi config trong Editor)
        /// </summary>
        public static void ReloadSequences()
        {
            _isInitialized = false;
            Initialize();
        }

        public static bool HasSteps(TutorialType type)
        {
            return SequencesByType.TryGetValue(type, out var sequence) && sequence.HasSteps;
        }

        public static bool IsCompleted(TutorialType type)
        {
            var savedIndex = LoadSavedIndex(type);
            return SequencesByType.TryGetValue(type, out var sequence) && savedIndex >= sequence.StepCount;
        }

        public static TutorialStepData GetCurrentStep(TutorialType type)
        {
            if (!SequencesByType.TryGetValue(type, out var sequence)) return null;

            var index = Mathf.Clamp(LoadSavedIndex(type), 0, sequence.StepCount);
            if (index >= sequence.StepCount) return null;

            var stepSo = sequence.GetStep(index);
            return stepSo != null ? ConvertToStepData(stepSo, index) : null;
        }

        /// <summary>
        ///     Lấy thông tin step hiện tại đang active
        /// </summary>
        public static TutorialStepData GetCurrentStepData()
        {
            if (!IsActive) return null;

            if (!SequencesByType.TryGetValue(ActiveType, out var sequence)) return null;

            var stepSo = sequence.GetStep(ActiveStepIndex);
            return stepSo != null ? ConvertToStepData(stepSo, ActiveStepIndex) : null;
        }

        public static RectTransform GetTarget(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;

            TargetsById.TryGetValue(targetId, out var target);
            return target;
        }

        public static void RegisterTarget(string targetId, RectTransform target)
        {
            if (string.IsNullOrEmpty(targetId)) return;

            if (target == null)
            {
                TargetsById.Remove(targetId);
                return;
            }

            TargetsById[targetId] = target;
        }

        public static bool TryStart(TutorialType type)
        {
            if (!HasSteps(type)) return false;

            var savedIndex = LoadSavedIndex(type);
            if (!SequencesByType.TryGetValue(type, out var sequence) || sequence.StepCount <= savedIndex) return false;

            IsActive = true;
            ActiveType = type;
            ActiveStepIndex = Mathf.Clamp(savedIndex, 0, sequence.StepCount - 1);

            var stepSo = sequence.GetStep(ActiveStepIndex);
            if (stepSo != null) StepChanged?.Invoke(ActiveType, ConvertToStepData(stepSo, ActiveStepIndex));

            return true;
        }

        public static void NextStep()
        {
            if (!IsActive) return;

            ActiveStepIndex++;
            SaveIndex(ActiveType, ActiveStepIndex);

            if (!SequencesByType.TryGetValue(ActiveType, out var sequence) || ActiveStepIndex >= sequence.StepCount)
            {
                IsActive = false;
                TutorialEnded?.Invoke(ActiveType);
                return;
            }

            var stepSO = sequence.GetStep(ActiveStepIndex);
            if (stepSO != null) StepChanged?.Invoke(ActiveType, ConvertToStepData(stepSO, ActiveStepIndex));
        }

        public static void ResetProgress(TutorialType type)
        {
            SaveIndex(type, 0);
            if (IsActive && ActiveType == type)
            {
                ActiveStepIndex = 0;
                if (SequencesByType.TryGetValue(type, out var sequence))
                {
                    var stepSo = sequence.GetStep(ActiveStepIndex);
                    if (stepSo != null) StepChanged?.Invoke(ActiveType, ConvertToStepData(stepSo, ActiveStepIndex));
                }
            }
        }

        public static void Complete(TutorialType type)
        {
            if (!HasSteps(type)) return;

            if (SequencesByType.TryGetValue(type, out var sequence)) SaveIndex(type, sequence.StepCount);

            if (IsActive && ActiveType == type)
            {
                IsActive = false;
                TutorialEnded?.Invoke(type);
            }
        }

        private static int LoadSavedIndex(TutorialType type)
        {
            return GPlayerPrefs.GetInt(PrefKeyPrefix + type);
        }

        private static void SaveIndex(TutorialType type, int index)
        {
            GPlayerPrefs.SetInt(PrefKeyPrefix + type, index);
            GPlayerPrefs.Save();
        }

        /// <summary>
        ///     Chuyển đổi TutorialStepSO sang TutorialStepData
        /// </summary>
        private static TutorialStepData ConvertToStepData(TutorialStepSO stepSO, int index)
        {
            var stepData = new TutorialStepData(
                index,
                stepSO.name,
                stepSO.message,
                stepSO.targetId
            )
            {
                FocusPadding = stepSO.focusPadding,
                BlockInput = stepSO.blockInput
            };

            return stepData;
        }
    }
}
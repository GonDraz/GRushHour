using System;
using GlobalState;
using GonDraz.Observable;
using GonDraz.PlayerPrefs;
using Puzzle.Data;
using UnityEngine;

namespace Managers
{
    /// <summary>
    ///     Static manager for puzzle level progression.
    ///     Loads all PuzzleLevelSO from Resources/Levels, tracks and persists current level index,
    ///     and provides helpers to play / advance levels.
    ///     (Manager tĩnh quản lý tiến trình level puzzle. Tải PuzzleLevelSO từ Resources/Levels,
    ///     lưu index level hiện tại và cung cấp các helper để chơi / tiến level.)
    /// </summary>
    public static class LevelManager
    {
        // Đường dẫn tương đối trong thư mục Resources
        private const string ResourcesPath = "Levels";

        // PlayerPrefs key cho index level hiện tại
        private const string CurrentLevelKey = "CurrentLevelIndex";

        private static PuzzleLevelSO[] _levels;

        /// <summary>
        ///     Index level hiện tại, tự động persist sang PlayerPrefs khi thay đổi.
        ///     (Current level index, auto-persisted to PlayerPrefs on change.)
        /// </summary>
        public static readonly GObservableValue<int> CurrentLevelIndex =
            GObservablePlayerPrefs.CreateObservableInt(CurrentLevelKey);

        static LevelManager()
        {
            LoadLevels();
        }

        // ─── Level loading ─────────────────────────────────────────────────────────

        /// <summary>
        ///     Tải tất cả PuzzleLevelSO từ Resources/Levels và sắp xếp theo tên.
        ///     (Loads all PuzzleLevelSO from Resources/Levels and sorts them by name.)
        /// </summary>
        private static void LoadLevels()
        {
            _levels = Resources.LoadAll<PuzzleLevelSO>(ResourcesPath);

            // Sắp xếp theo tên để đảm bảo thứ tự: Level 1 < Level 2 < Level 3 …
            // Sort alphabetically so order is deterministic regardless of load order.
            Array.Sort(_levels, (a, b) =>
                string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            Debug.Log($"[LevelManager] Loaded {_levels.Length} levels from Resources/{ResourcesPath}.");
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>Tổng số level được tải. (Total number of loaded levels.)</summary>
        public static int GetTotalLevelCount() => _levels?.Length ?? 0;

        /// <summary>
        ///     Trả về PuzzleLevelSO tại index chỉ định, hoặc null nếu ngoài phạm vi.
        ///     (Returns the PuzzleLevelSO at the given index, or null if out of range.)
        /// </summary>
        public static PuzzleLevelSO GetLevel(int index)
        {
            if (_levels == null || index < 0 || index >= _levels.Length)
                return null;
            return _levels[index];
        }

        /// <summary>Trả về PuzzleLevelSO đang được chọn. (Returns the currently selected level.)</summary>
        public static PuzzleLevelSO GetCurrentLevel() => GetLevel(CurrentLevelIndex.Value);

        /// <summary>
        ///     Chọn level theo index (clamp trong phạm vi hợp lệ, tự động persist).
        ///     (Sets the current level index, clamped to valid range, auto-persisted.)
        /// </summary>
        public static void SetCurrentLevel(int index)
        {
            CurrentLevelIndex.Value = Mathf.Clamp(index, 0, Mathf.Max(0, GetTotalLevelCount() - 1));
        }

        /// <summary>
        ///     Tải level hiện tại vào PuzzleGameController và bắt đầu gameplay.
        ///     PuzzleGameController.SetupLevel() sẽ tự đọc level từ LevelManager.
        ///     (Starts gameplay with the current level. PuzzleGameController.SetupLevel()
        ///     reads the level directly from LevelManager.)
        /// </summary>
        public static void PlayCurrentLevel()
        {
            if (GetCurrentLevel() == null)
            {
                Debug.LogError($"[LevelManager] No level found at index {CurrentLevelIndex.Value}.");
                return;
            }


            GlobalStateMachine.Change<GlobalStateMachine.InGameState>(false);
            EventManager.SetupGameplay.Invoke();
        }

        /// <summary>
        ///     Tiến đến level tiếp theo rồi bắt đầu chơi. Quay về level 0 sau level cuối.
        ///     (Advances to the next level and starts it. Wraps to level 0 after the last.)
        /// </summary>
        public static void PlayNextLevel()
        {
            AdvanceLevel();
            PlayCurrentLevel();
        }

        // ─── Internal helpers ─────────────────────────────────────────────────────

        /// <summary>
        ///     Tăng CurrentLevelIndex thêm 1, quay vòng về 0 sau level cuối.
        ///     (Increments CurrentLevelIndex by 1, wrapping back to 0 after the last level.)
        /// </summary>
        private static void AdvanceLevel()
        {
            var next = CurrentLevelIndex.Value + 1;
            CurrentLevelIndex.Value = next < GetTotalLevelCount() ? next : 0;
        }
    }
}



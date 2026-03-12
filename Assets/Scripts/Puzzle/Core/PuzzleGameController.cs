using System;
using System.Collections;
using GonDraz.Base;
using Managers;
using Puzzle.Data;
using UnityEngine;

namespace Puzzle.Core
{
    public class PuzzleGameController : BaseBehaviour
    {
        [SerializeField] private PuzzleLevelSO level;
        [SerializeField] private bool setupOnStart = true;

        public PuzzleBoardState Board { get; } = new();

        private void Start()
        {
            if (setupOnStart)
                SetupLevel();
        }

        public event Action BoardInitialized;
        public event Action<string, Vector2Int> BlockMoved;
        public event Action Solved;

        public override bool SubscribeUsingOnEnable()
        {
            return true;
        }

        public override bool UnsubscribeUsingOnDisable()
        {
            return true;
        }

        public override void Subscribe()
        {
            base.Subscribe();
            EventManager.SetupGameplay += OnSetupGameplay;
            EventManager.AutoSolveToggle += OnAutoSolveToggle;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            EventManager.SetupGameplay -= OnSetupGameplay;
            EventManager.AutoSolveToggle -= OnAutoSolveToggle;
        }

        private void OnSetupGameplay()
        {
            // Dừng auto-solve khi load level mới (Stop auto-solve when a new level loads)
            StopAutoSolve();
            SetupLevel();
        }

        private void OnAutoSolveToggle()
        {
            if (IsAutoSolving) StopAutoSolve();
            else StartAutoSolve();
        }

        public void SetLevel(PuzzleLevelSO puzzleLevel)
        {
            level = puzzleLevel;
        }

        private bool _isSolved;

        public bool SetupLevel()
        {
            _isSolved = false;
            // Ưu tiên level từ LevelManager, fallback về serialized field nếu chưa có level nào trong Resources
            // Prefer level from LevelManager; fall back to the serialized field if LevelManager returns null.
            var levelToLoad = LevelManager.GetCurrentLevel() ?? level;

            if (!Board.Initialize(levelToLoad, out var message))
            {
                Debug.LogError($"[Puzzle] Setup failed: {message}", this);
                return false;
            }

            EventManager.PuzzleInitialized.Invoke();
            BoardInitialized?.Invoke();
            return true;
        }

        public bool TryMoveBlock(string blockId, Vector2Int direction, int requestedSteps = 1)
        {
            if (_isSolved) return false;

            if (!Board.TryMove(blockId, direction, requestedSteps, out _, out var message))
            {
                EventManager.PuzzleInvalidMove.Invoke(blockId);
                Debug.Log($"[Puzzle] Invalid move: {message}");
                return false;
            }

            if (!Board.TryGetBlock(blockId, out var block))
                return false;

            EventManager.PuzzleBlockMoved.Invoke(blockId, block.Origin);
            BlockMoved?.Invoke(blockId, block.Origin);

            if (!Board.IsSolved())
                return true;

            _isSolved = true;

            // Thông báo presenter bắt đầu animation exit trước khi fire PuzzleSolved
            // Notify presenter to play the exit animation before PuzzleSolved fires.
            Solved?.Invoke();

            // Fallback: không có presenter → fire PuzzleSolved ngay lập tức
            // Fallback: no presenter subscribed → fire PuzzleSolved immediately.
            if (Solved == null)
                EventManager.PuzzleSolved.Invoke();

            return true;
        }

        // ─── Auto-solve ───────────────────────────────────────────────────────────

        private Coroutine _autoSolveCoroutine;

        /// <summary>Đang chạy auto-solve hay không. (Whether auto-solve is currently running.)</summary>
        public bool IsAutoSolving => _autoSolveCoroutine != null;

        /// <summary>
        ///     Chạy BFS tìm lời giải rồi tự động thực hiện từng nước với khoảng cách <paramref name="moveInterval" /> giây.
        ///     (Runs BFS to find a solution, then replays moves automatically at <paramref name="moveInterval" />-second intervals.)
        /// </summary>
        public void StartAutoSolve(float moveInterval = 0.6f)
        {
            if (_autoSolveCoroutine != null)
            {
                StopCoroutine(_autoSolveCoroutine);
                _autoSolveCoroutine = null;
            }
            _autoSolveCoroutine = StartCoroutine(AutoSolveCoroutine(moveInterval));
            EventManager.AutoSolvingStateChanged.Invoke(true);
        }

        /// <summary>Dừng auto-solve nếu đang chạy. (Stops the auto-solve coroutine if running.)</summary>
        public void StopAutoSolve()
        {
            if (_autoSolveCoroutine == null) return;
            StopCoroutine(_autoSolveCoroutine);
            _autoSolveCoroutine = null;
            EventManager.AutoSolvingStateChanged.Invoke(false);
        }

        private IEnumerator AutoSolveCoroutine(float moveInterval)
        {
            // Chạy BFS đồng bộ trên main thread (nhanh với board 6×6 thông thường)
            // Runs synchronous BFS on the main thread (fast for typical 6×6 boards)
            var solution = PuzzleSolver.Solve(Board);

            if (solution == null || solution.Count == 0)
            {
                Debug.LogWarning("[Puzzle] Auto-solve: no solution found or board already solved.");
                _autoSolveCoroutine = null;
                EventManager.AutoSolvingStateChanged.Invoke(false);
                yield break;
            }

            Debug.Log($"[Puzzle] Auto-solve: replaying {solution.Count} moves...");

            foreach (var move in solution)
            {
                TryMoveBlock(move.BlockId, move.Direction, move.Steps);
                yield return new WaitForSeconds(moveInterval);

                // Dừng sớm nếu puzzle đã được giải (ví dụ user tự giải trong lúc chờ)
                // Early-exit if the board is solved mid-replay
                if (Board.IsSolved()) break;
            }

            _autoSolveCoroutine = null;
            EventManager.AutoSolvingStateChanged.Invoke(false);
        }
    }
}

using System;
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
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            EventManager.SetupGameplay -= OnSetupGameplay;
        }

        private void OnSetupGameplay()
        {
            SetupLevel();
        }

        public void SetLevel(PuzzleLevelSO puzzleLevel)
        {
            level = puzzleLevel;
        }

        public bool SetupLevel()
        {
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

            EventManager.PuzzleSolved.Invoke();
            Solved?.Invoke();
            return true;
        }
    }
}
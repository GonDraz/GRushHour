using System;
using System.Collections.Generic;
using Puzzle.Data;
using UnityEngine;

namespace Puzzle.Core
{
    public sealed class PuzzleBlockState
    {
        public PuzzleBlockState(BlockDefinition definition)
        {
            Definition = definition;
            Origin = definition.startCell;
        }

        public BlockDefinition Definition { get; }
        public Vector2Int Origin { get; set; }
    }

    public sealed class PuzzleBoardState
    {
        private readonly Dictionary<string, PuzzleBlockState> _blocks = new(StringComparer.Ordinal);
        private readonly List<Vector2Int> _cellBuffer = new(4);
        private readonly List<ExitGateDefinition> _exits = new();
        private readonly HashSet<Vector2Int> _occupiedCells = new();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string TargetBlockId { get; private set; }

        public IReadOnlyDictionary<string, PuzzleBlockState> Blocks => _blocks;
        public IReadOnlyList<ExitGateDefinition> Exits => _exits;

        public bool Initialize(PuzzleLevelSO level, out string message)
        {
            if (level == null)
            {
                message = "Level is null.";
                return false;
            }

            if (!level.Validate(out message))
                return false;

            Width = level.boardSize.x;
            Height = level.boardSize.y;
            TargetBlockId = level.targetBlockId;

            _blocks.Clear();
            _exits.Clear();
            _exits.AddRange(level.exits);

            foreach (var blockDef in level.blocks)
                _blocks[blockDef.id] = new PuzzleBlockState(blockDef);

            if (!RebuildOccupancy(out message))
                return false;

            message = "Board initialized.";
            return true;
        }

        public bool TryGetBlock(string blockId, out PuzzleBlockState blockState)
        {
            return _blocks.TryGetValue(blockId, out blockState);
        }

        /// <summary>
        /// Readonly probe: how many steps can <paramref name="blockId"/> move in
        /// <paramref name="direction"/> without committing to board state.
        /// Returns 0 if blocked or axis is disallowed by MoveRule.
        /// Mirrors StoneMovement.GetValidMoveRange (3D raycast → grid occupancy probe).
        /// </summary>
        public int QueryMaxSteps(string blockId, Vector2Int direction)
        {
            if (!_blocks.TryGetValue(blockId, out var blockState))
                return 0;

            var dir = NormalizeDirection(direction);
            if (dir == Vector2Int.zero)
                return 0;

            if (!CanMoveByRule(blockState.Definition.moveRule, dir))
                return 0;

            var steps = 0;
            var probe = blockState.Origin;
            var maxProbe = Width + Height; // upper bound

            for (var i = 0; i < maxProbe; i++)
            {
                var next = probe + dir;
                if (!CanOccupy(blockId, blockState.Definition.shape, blockState.Definition.orientation, next))
                    break;
                probe = next;
                steps++;
            }

            return steps;
        }

        public bool TryMove(string blockId, Vector2Int direction, int requestedSteps, out int movedSteps,
            out string message)
        {
            movedSteps = 0;

            if (!_blocks.TryGetValue(blockId, out var blockState))
            {
                message = $"Block '{blockId}' not found.";
                return false;
            }

            direction = NormalizeDirection(direction);
            if (direction == Vector2Int.zero)
            {
                message = "Direction cannot be zero.";
                return false;
            }

            if (!CanMoveByRule(blockState.Definition.moveRule, direction))
            {
                message = $"Block '{blockId}' cannot move in this axis.";
                return false;
            }

            var maxSteps = Mathf.Max(1, requestedSteps);
            var probeOrigin = blockState.Origin;

            for (var i = 0; i < maxSteps; i++)
            {
                var next = probeOrigin + direction;
                if (!CanOccupy(blockId, blockState.Definition.shape, blockState.Definition.orientation, next))
                    break;

                probeOrigin = next;
                movedSteps++;
            }

            if (movedSteps == 0)
            {
                message = $"Block '{blockId}' is blocked.";
                return false;
            }

            blockState.Origin = probeOrigin;
            RebuildOccupancy(out _);

            message = "Move applied.";
            return true;
        }

        public bool IsSolved()
        {
            if (!_blocks.TryGetValue(TargetBlockId, out var target))
                return false;

            PuzzleFootprintUtility.FillCells(target.Definition.shape, target.Definition.orientation, target.Origin,
                _cellBuffer);

            foreach (var exit in _exits)
                if (TouchesExit(_cellBuffer, exit))
                    return true;

            return false;
        }

        private bool RebuildOccupancy(out string message)
        {
            _occupiedCells.Clear();

            foreach (var pair in _blocks)
            {
                var block = pair.Value;
                PuzzleFootprintUtility.FillCells(block.Definition.shape, block.Definition.orientation, block.Origin,
                    _cellBuffer);

                foreach (var cell in _cellBuffer)
                {
                    if (!IsInsideBoard(cell))
                    {
                        message = $"Block '{pair.Key}' is out of board at cell {cell}.";
                        return false;
                    }

                    if (!_occupiedCells.Add(cell))
                    {
                        message = $"Overlap detected at cell {cell}.";
                        return false;
                    }
                }
            }

            message = "Occupancy rebuilt.";
            return true;
        }

        private bool CanOccupy(string movingBlockId, BlockShapeType shape, BlockOrientation orientation,
            Vector2Int origin)
        {
            // GetCurrentCells must be called BEFORE FillCells because both share _cellBuffer:
            // FillCells calls buffer.Clear() internally, so calling GetCurrentCells after FillCells
            // would overwrite _cellBuffer with the block's current cells, making the foreach below
            // iterate the wrong (current) cells instead of the new target position cells.
            var movingCurrentCells = GetCurrentCells(movingBlockId);

            PuzzleFootprintUtility.FillCells(shape, orientation, origin, _cellBuffer);

            foreach (var cell in _cellBuffer)
            {
                if (!IsInsideBoard(cell))
                    return false;

                if (!_occupiedCells.Contains(cell))
                    continue;

                if (movingCurrentCells.Contains(cell))
                    continue;

                return false;
            }

            return true;
        }

        private HashSet<Vector2Int> GetCurrentCells(string blockId)
        {
            var result = new HashSet<Vector2Int>();

            if (!_blocks.TryGetValue(blockId, out var block))
                return result;

            PuzzleFootprintUtility.FillCells(block.Definition.shape, block.Definition.orientation, block.Origin,
                _cellBuffer);
            foreach (var cell in _cellBuffer)
                result.Add(cell);

            return result;
        }

        private bool IsInsideBoard(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
        }

        private static bool CanMoveByRule(MoveAxisRule rule, Vector2Int direction)
        {
            if (direction.x != 0)
                return PuzzleFootprintUtility.SupportsHorizontalMove(rule);

            if (direction.y != 0)
                return PuzzleFootprintUtility.SupportsVerticalMove(rule);

            return false;
        }

        private static Vector2Int NormalizeDirection(Vector2Int direction)
        {
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
                return new Vector2Int(Math.Sign(direction.x), 0);

            return new Vector2Int(0, Math.Sign(direction.y));
        }

        private bool TouchesExit(List<Vector2Int> cells, ExitGateDefinition exit)
        {
            foreach (var cell in cells)
                switch (exit.edge)
                {
                    case ExitEdge.Left:
                        if (cell.x == 0 && exit.ContainsIndex(cell.y))
                            return true;
                        break;
                    case ExitEdge.Right:
                        if (cell.x == Width - 1 && exit.ContainsIndex(cell.y))
                            return true;
                        break;
                    case ExitEdge.Top:
                        if (cell.y == Height - 1 && exit.ContainsIndex(cell.x))
                            return true;
                        break;
                    case ExitEdge.Bottom:
                        if (cell.y == 0 && exit.ContainsIndex(cell.x))
                            return true;
                        break;
                }

            return false;
        }
    }
}
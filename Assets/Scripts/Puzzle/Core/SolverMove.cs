using UnityEngine;

namespace Puzzle.Core
{
    /// <summary>
    ///     Một nước đi trong lời giải: trượt block <see cref="BlockId" /> theo
    ///     <see cref="Direction" /> một số <see cref="Steps" /> ô.
    ///     (A single step in a solution: slide <see cref="BlockId" /> in
    ///     <see cref="Direction" /> by <see cref="Steps" /> cells.)
    /// </summary>
    public readonly struct SolverMove
    {
        public readonly string BlockId;
        public readonly Vector2Int Direction;
        public readonly int Steps;

        public SolverMove(string blockId, Vector2Int direction, int steps)
        {
            BlockId = blockId;
            Direction = direction;
            Steps = steps;
        }

        public override string ToString() =>
            $"[{BlockId}] {DirectionName(Direction)} x{Steps}";

        private static string DirectionName(Vector2Int d)
        {
            if (d == Vector2Int.right) return "Right";
            if (d == Vector2Int.left) return "Left";
            if (d == Vector2Int.up) return "Up";
            if (d == Vector2Int.down) return "Down";
            return d.ToString();
        }
    }
}


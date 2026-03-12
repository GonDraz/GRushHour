using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Core
{
    public static class PuzzleFootprintUtility
    {
        public static List<Vector2Int> GetCells(BlockShapeType shape, BlockOrientation orientation, Vector2Int origin)
        {
            var cells = new List<Vector2Int>(4);
            FillCells(shape, orientation, origin, cells);
            return cells;
        }

        public static void FillCells(BlockShapeType shape, BlockOrientation orientation, Vector2Int origin,
            List<Vector2Int> buffer)
        {
            buffer.Clear();

            switch (shape)
            {
                case BlockShapeType.Single1x1:
                    buffer.Add(origin);
                    break;
                case BlockShapeType.Line1x2:
                    if (orientation == BlockOrientation.Vertical)
                    {
                        buffer.Add(origin);
                        buffer.Add(origin + Vector2Int.up);
                    }
                    else
                    {
                        buffer.Add(origin);
                        buffer.Add(origin + Vector2Int.right);
                    }

                    break;
                case BlockShapeType.Line1x3:
                    if (orientation == BlockOrientation.Vertical)
                    {
                        buffer.Add(origin);
                        buffer.Add(origin + Vector2Int.up);
                        buffer.Add(origin + Vector2Int.up * 2);
                    }
                    else
                    {
                        buffer.Add(origin);
                        buffer.Add(origin + Vector2Int.right);
                        buffer.Add(origin + Vector2Int.right * 2);
                    }

                    break;
                case BlockShapeType.Square2x2:
                    buffer.Add(origin);
                    buffer.Add(origin + Vector2Int.right);
                    buffer.Add(origin + Vector2Int.up);
                    buffer.Add(origin + Vector2Int.right + Vector2Int.up);
                    break;
                case BlockShapeType.Corner2x2MissingOne:
                    FillCornerCells(orientation, origin, buffer);
                    break;
            }
        }

        private static void FillCornerCells(BlockOrientation orientation, Vector2Int origin, List<Vector2Int> buffer)
        {
            var bottomLeft = origin;
            var bottomRight = origin + Vector2Int.right;
            var topLeft = origin + Vector2Int.up;
            var topRight = origin + Vector2Int.right + Vector2Int.up;

            // 2x2 block with one corner removed.
            switch (orientation)
            {
                case BlockOrientation.MissingTopLeft:
                    buffer.Add(bottomLeft);
                    buffer.Add(bottomRight);
                    buffer.Add(topRight);
                    break;
                case BlockOrientation.MissingTopRight:
                    buffer.Add(bottomLeft);
                    buffer.Add(bottomRight);
                    buffer.Add(topLeft);
                    break;
                case BlockOrientation.MissingBottomLeft:
                    buffer.Add(bottomRight);
                    buffer.Add(topLeft);
                    buffer.Add(topRight);
                    break;
                case BlockOrientation.MissingBottomRight:
                    buffer.Add(bottomLeft);
                    buffer.Add(topLeft);
                    buffer.Add(topRight);
                    break;
                default:
                    buffer.Add(bottomLeft);
                    buffer.Add(bottomRight);
                    buffer.Add(topRight);
                    break;
            }
        }

        public static bool SupportsHorizontalMove(MoveAxisRule moveRule)
        {
            return moveRule == MoveAxisRule.HorizontalOnly || moveRule == MoveAxisRule.Both;
        }

        public static bool SupportsVerticalMove(MoveAxisRule moveRule)
        {
            return moveRule == MoveAxisRule.VerticalOnly || moveRule == MoveAxisRule.Both;
        }
    }
}
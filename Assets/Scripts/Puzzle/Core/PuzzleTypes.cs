using System;

namespace Puzzle.Core
{
    public enum BlockShapeType
    {
        Single1x1 = 0,
        Line1x2 = 1,
        Line1x3 = 2,
        Square2x2 = 3,
        Corner2x2MissingOne = 4
    }

    public enum BlockOrientation
    {
        Horizontal = 0,
        Vertical = 1,
        MissingTopLeft = 2,
        MissingTopRight = 3,
        MissingBottomLeft = 4,
        MissingBottomRight = 5
    }

    public enum MoveAxisRule
    {
        HorizontalOnly = 0,
        VerticalOnly = 1,
        Both = 2
    }

    public enum ExitEdge
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3
    }

    [Serializable]
    public struct CellPos
    {
        public int x;
        public int y;

        public CellPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
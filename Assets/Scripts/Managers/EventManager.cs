using GonDraz.Events;
using UnityEngine;

namespace Managers
{
    public static class EventManager
    {
        public static GEvent SetupGameplay = new("SetupGameplay");
        public static GEvent PuzzleInitialized = new("PuzzleInitialized");

        public static GEvent<string, Vector2Int> PuzzleBlockMoved =
            new("PuzzleBlockMoved");

        public static GEvent<string> PuzzleInvalidMove = new("PuzzleInvalidMove");
        public static GEvent PuzzleSolved = new("PuzzleSolved");

        // Kích hoạt / dừng auto-solve từ UI (Toggle auto-solve from UI)
        public static GEvent AutoSolveToggle = new("AutoSolveToggle");

        // Phát ra khi trạng thái auto-solve thay đổi: true = đang chạy, false = dừng
        // Fired when auto-solve state changes: true = running, false = stopped
        public static GEvent<bool> AutoSolvingStateChanged = new("AutoSolvingStateChanged");
    }
}
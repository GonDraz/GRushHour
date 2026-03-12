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
    }
}
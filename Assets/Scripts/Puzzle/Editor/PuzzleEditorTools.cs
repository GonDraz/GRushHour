#if UNITY_EDITOR
using System.IO;
using Puzzle.Core;
using Puzzle.Data;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class PuzzleEditorTools
    {
        [MenuItem("Tools/Puzzle/Create Sample Level")]
        public static void CreateSampleLevel()
        {
            var level = ScriptableObject.CreateInstance<PuzzleLevelSO>();
            level.boardSize = new Vector2Int(6, 6);
            level.targetBlockId = "target";

            level.blocks.Add(new BlockDefinition
            {
                id = "target",
                isTarget = true,
                shape = BlockShapeType.Line1x2,
                orientation = BlockOrientation.Horizontal,
                moveRule = MoveAxisRule.HorizontalOnly,
                startCell = new Vector2Int(1, 2)
            });

            level.blocks.Add(new BlockDefinition
            {
                id = "v1",
                shape = BlockShapeType.Line1x3,
                orientation = BlockOrientation.Vertical,
                moveRule = MoveAxisRule.VerticalOnly,
                startCell = new Vector2Int(4, 1)
            });

            level.blocks.Add(new BlockDefinition
            {
                id = "corner",
                shape = BlockShapeType.Corner2x2MissingOne,
                orientation = BlockOrientation.MissingTopLeft,
                moveRule = MoveAxisRule.Both,
                startCell = new Vector2Int(2, 4)
            });

            level.exits.Add(new ExitGateDefinition
            {
                id = "right_main",
                edge = ExitEdge.Right,
                startIndex = 2,
                length = 2,
                onlyTarget = true
            });

            level.exits.Add(new ExitGateDefinition
            {
                id = "top_bonus",
                edge = ExitEdge.Top,
                startIndex = 1,
                length = 1,
                onlyTarget = true
            });

            var folder = "Assets/Scripts/Puzzle/Levels";
            if (!AssetDatabase.IsValidFolder(folder))
                Directory.CreateDirectory(folder);

            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, "SamplePuzzleLevel.asset"));
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = level;
            EditorGUIUtility.PingObject(level);
            Debug.Log($"[Puzzle] Created sample level: {path}", level);

            // Open in Level Editor immediately
            PuzzleLevelEditorWindow.Open(level);
        }
    }
}
#endif
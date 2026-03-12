using System;
using System.Collections.Generic;
using Puzzle.Core;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Puzzle.Data
{
    [Serializable]
    public class BlockDefinition
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("Row", Width = 120)]
#endif
        public string id = "block";
#if ODIN_INSPECTOR
        [HorizontalGroup("Row", Width = 70)]
#endif
        public bool isTarget;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public Vector2Int startCell;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public BlockShapeType shape = BlockShapeType.Line1x2;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public BlockOrientation orientation = BlockOrientation.Horizontal;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public MoveAxisRule moveRule = MoveAxisRule.HorizontalOnly;
    }

    [Serializable]
    public class ExitGateDefinition
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("Row", Width = 120)]
#endif
        public string id = "exit";
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public ExitEdge edge = ExitEdge.Right;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public int startIndex;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public int length = 1;
#if ODIN_INSPECTOR
        [HorizontalGroup("Row")]
#endif
        public bool onlyTarget = true;

        public bool ContainsIndex(int index)
        {
            return index >= startIndex && index < startIndex + Mathf.Max(1, length);
        }
    }

    [CreateAssetMenu(fileName = "PuzzleLevel", menuName = "Puzzle/Puzzle Level", order = 0)]
    public class PuzzleLevelSO : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("Board")] [MinValue(1)]
#else
        [Header("Board")]
#endif
        public Vector2Int boardSize = new(6, 6);

#if ODIN_INSPECTOR
        [Title("Target")]
#else
        [Header("Target")]
#endif
        public string targetBlockId = "target";

#if ODIN_INSPECTOR
        [Title("Blocks")] [TableList(AlwaysExpanded = true, DrawScrollView = true)]
#else
        [Header("Blocks")]
#endif
        public List<BlockDefinition> blocks = new();

#if ODIN_INSPECTOR
        [Title("Exits")] [TableList(AlwaysExpanded = true, DrawScrollView = true)]
#else
        [Header("Exits")]
#endif
        public List<ExitGateDefinition> exits = new();

        public bool Validate(out string message)
        {
            if (boardSize.x <= 0 || boardSize.y <= 0)
            {
                message = "Board size must be > 0.";
                return false;
            }

            if (blocks == null || blocks.Count == 0)
            {
                message = "No blocks configured.";
                return false;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            var targetCount = 0;

            foreach (var block in blocks)
            {
                if (block == null)
                {
                    message = "Block contains null entry.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(block.id))
                {
                    message = "Block id is empty.";
                    return false;
                }

                if (!ids.Add(block.id))
                {
                    message = $"Duplicate block id: {block.id}";
                    return false;
                }

                if (block.isTarget)
                    targetCount++;
            }

            if (targetCount != 1)
            {
                message = "Exactly one block must be marked as target.";
                return false;
            }

            if (!ids.Contains(targetBlockId))
            {
                message = $"targetBlockId '{targetBlockId}' is not found in blocks.";
                return false;
            }

            if (exits == null || exits.Count == 0)
            {
                message = "At least one exit gate is required.";
                return false;
            }

            foreach (var exit in exits)
            {
                if (exit == null)
                {
                    message = "Exit list contains null entry.";
                    return false;
                }

                if (exit.length <= 0)
                {
                    message = $"Exit '{exit.id}' length must be > 0.";
                    return false;
                }
            }

            message = "Level config is valid.";
            return true;
        }
    }
}
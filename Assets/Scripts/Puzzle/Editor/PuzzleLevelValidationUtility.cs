#if UNITY_EDITOR
using System.Collections.Generic;
using Puzzle.Core;
using Puzzle.Data;
using UnityEngine;

namespace Puzzle.Editor
{
    /// <summary>
    /// Shared validation rules for PuzzleLevelSO.
    /// Collects ALL errors (unlike PuzzleLevelSO.Validate which stops at the first).
    /// </summary>
    public static class PuzzleLevelValidationUtility
    {
        public static bool Validate(PuzzleLevelSO level, List<string> errors)
        {
            errors.Clear();

            if (level == null)
            {
                errors.Add("Level asset is null.");
                return false;
            }

            // ── Board size ────────────────────────────────────────────────────────────
            if (level.boardSize.x <= 0 || level.boardSize.y <= 0)
                errors.Add($"Board size must be > 0 (current: {level.boardSize}).");

            // ── Blocks ────────────────────────────────────────────────────────────────
            if (level.blocks == null || level.blocks.Count == 0)
            {
                errors.Add("No blocks configured.");
            }
            else
            {
                var ids = new HashSet<string>(System.StringComparer.Ordinal);
                var targetCount = 0;

                foreach (var block in level.blocks)
                {
                    if (block == null) { errors.Add("Block list contains a null entry."); continue; }

                    if (string.IsNullOrWhiteSpace(block.id))
                    {
                        errors.Add("A block has an empty id.");
                        continue;
                    }

                    if (!ids.Add(block.id))
                        errors.Add($"Duplicate block id: '{block.id}'.");

                    if (block.isTarget)
                        targetCount++;
                }

                if (targetCount == 0)
                    errors.Add("No block is marked as target (isTarget = true).");
                else if (targetCount > 1)
                    errors.Add($"{targetCount} blocks are marked as target — exactly one is required.");

                if (!ids.Contains(level.targetBlockId))
                    errors.Add($"targetBlockId '{level.targetBlockId}' does not match any block id.");
            }

            // ── Exits ─────────────────────────────────────────────────────────────────
            if (level.exits == null || level.exits.Count == 0)
            {
                errors.Add("At least one exit gate is required.");
            }
            else
            {
                var exitIds = new HashSet<string>(System.StringComparer.Ordinal);
                foreach (var exit in level.exits)
                {
                    if (exit == null) { errors.Add("Exit list contains a null entry."); continue; }

                    if (string.IsNullOrWhiteSpace(exit.id))
                        errors.Add("An exit gate has an empty id.");
                    else if (!exitIds.Add(exit.id))
                        errors.Add($"Duplicate exit id: '{exit.id}'.");

                    if (exit.length <= 0)
                        errors.Add($"Exit '{exit.id}': length must be > 0 (current: {exit.length}).");

                    // Check index in bounds
                    if (level.boardSize.x > 0 && level.boardSize.y > 0)
                    {
                        var axisSize = (exit.edge == ExitEdge.Left || exit.edge == ExitEdge.Right)
                            ? level.boardSize.y  // index is a row
                            : level.boardSize.x; // index is a column

                        if (exit.startIndex < 0 || exit.startIndex >= axisSize)
                            errors.Add($"Exit '{exit.id}': startIndex {exit.startIndex} is outside board bounds (0–{axisSize - 1}).");
                        else if (exit.startIndex + exit.length > axisSize)
                            errors.Add($"Exit '{exit.id}': extends beyond board edge (startIndex {exit.startIndex} + length {exit.length} > {axisSize}).");
                    }
                }
            }

            // ── Cell occupancy + bounds ───────────────────────────────────────────────
            if (level.boardSize.x > 0 && level.boardSize.y > 0 && level.blocks != null)
            {
                var occupied = new Dictionary<Vector2Int, string>();
                var cellBuffer = new List<Vector2Int>(4);

                foreach (var block in level.blocks)
                {
                    if (block == null || string.IsNullOrWhiteSpace(block.id)) continue;

                    PuzzleFootprintUtility.FillCells(
                        block.shape, block.orientation, block.startCell, cellBuffer);

                    foreach (var cell in cellBuffer)
                    {
                        if (cell.x < 0 || cell.x >= level.boardSize.x ||
                            cell.y < 0 || cell.y >= level.boardSize.y)
                        {
                            errors.Add(
                                $"Block '{block.id}' has a cell {cell} outside board bounds " +
                                $"({level.boardSize.x}×{level.boardSize.y}).");
                        }
                        else if (occupied.TryGetValue(cell, out var otherId))
                        {
                            errors.Add(
                                $"Block '{block.id}' overlaps block '{otherId}' at cell {cell}.");
                        }
                        else
                        {
                            occupied[cell] = block.id;
                        }
                    }
                }
            }

            return errors.Count == 0;
        }
    }
}
#endif


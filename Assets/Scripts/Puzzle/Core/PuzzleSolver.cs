using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Puzzle.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Puzzle.Core
{
    /// <summary>
    ///     Thuật toán BFS giải puzzle Rush-Hour.
    ///     Trả về chuỗi nước đi ngắn nhất (tính theo số lần di chuyển, không phải số ô),
    ///     hoặc <c>null</c> nếu không có lời giải / vượt giới hạn node.
    ///     (BFS solver for Rush-Hour-style puzzles.
    ///     Returns the shortest move sequence (by move count, not step count),
    ///     or <c>null</c> if unsolvable or the node limit is exceeded.)
    /// </summary>
    public static class PuzzleSolver
    {
        // Cached direction arrays — tránh cấp phát mỗi lần (avoid per-call alloc)
        private static readonly Vector2Int[] Horizontal = { Vector2Int.right, Vector2Int.left };
        private static readonly Vector2Int[] Vertical = { Vector2Int.up, Vector2Int.down };
        private static readonly Vector2Int[] AllDirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        ///     Tìm lời giải BFS cho <paramref name="board" />.
        ///     (Finds the BFS solution for <paramref name="board" />.)
        /// </summary>
        /// <param name="board">Trạng thái bàn cờ hiện tại. (Current board state.)</param>
        /// <param name="maxNodes">
        ///     Giới hạn số node BFS để tránh treo game với level cực khó.
        ///     (BFS node cap to prevent freezing on extremely hard levels.)
        /// </param>
        public static List<SolverMove> Solve(PuzzleBoardState board, int maxNodes = 200_000)
        {
            var sw = Stopwatch.StartNew();

            // ── Trích dữ liệu bất biến từ board (Extract immutable data) ──
            var defs = new Dictionary<string, BlockDefinition>(StringComparer.Ordinal);
            var initialOrigins = new Dictionary<string, Vector2Int>(StringComparer.Ordinal);

            foreach (var kv in board.Blocks)
            {
                defs[kv.Key] = kv.Value.Definition;
                initialOrigins[kv.Key] = kv.Value.Origin;
            }

            var width = board.Width;
            var height = board.Height;
            var targetId = board.TargetBlockId;
            var exits = board.Exits;

            // Sắp xếp ID một lần để mã hoá state ổn định
            // Pre-sort IDs once for a stable state encoding
            var sortedIds = new string[defs.Count];
            defs.Keys.CopyTo(sortedIds, 0);
            Array.Sort(sortedIds, StringComparer.Ordinal);

            // ── Trường hợp đã giải rồi (Already solved) ──
            if (IsSolved(targetId, initialOrigins, defs, exits, width, height))
            {
                Debug.Log("[PuzzleSolver] Board is already solved.");
                return new List<SolverMove>();
            }

            // ── BFS ──────────────────────────────────────────────────────────────
            var startKey = EncodeState(sortedIds, initialOrigins);

            // cameFrom[stateKey] = (parentKey, move dẫn đến state này)
            var cameFrom = new Dictionary<string, (string parent, SolverMove move)>(StringComparer.Ordinal)
            {
                [startKey] = (null, default)
            };

            // Frontier: (stateKey, snapshot origins để sinh nước đi)
            var frontier = new Queue<(string key, Dictionary<string, Vector2Int> origins)>();
            frontier.Enqueue((startKey, initialOrigins));

            string solvedKey = null;

            while (frontier.Count > 0)
            {
                // Kiểm tra giới hạn node — (Node-cap check)
                if (cameFrom.Count > maxNodes)
                {
                    Debug.LogWarning($"[PuzzleSolver] Exceeded {maxNodes} nodes. Level may be too complex.");
                    return null;
                }

                var (currentKey, origins) = frontier.Dequeue();

                // Tính occupancy một lần cho toàn bộ block trong state này
                // Build full occupancy once per state (shared across block loop)
                var occupied = BuildOccupancy(origins, defs);

                foreach (var blockId in sortedIds)
                {
                    var def = defs[blockId];
                    var blockOrigin = origins[blockId];
                    // Ô ban đầu của block đang di chuyển — bỏ qua khi check va chạm
                    var movingCells = GetCellSet(def, blockOrigin);

                    foreach (var dir in GetDirections(def.moveRule))
                    {
                        var probe = blockOrigin;

                        // Thử từng bước 1..max cho hướng này
                        // Try each step count for this direction
                        for (var steps = 1; steps <= width + height; steps++)
                        {
                            probe += dir;
                            var nextCells = PuzzleFootprintUtility.GetCells(def.shape, def.orientation, probe);

                            // Nếu bị chặn thì các bước xa hơn cũng không đi được
                            // Blocked → further steps in this direction are also impossible
                            if (!IsValidPlacement(nextCells, occupied, movingCells, width, height))
                                break;

                            // Kiểm tra đã thăm chưa mà không cấp phát dict mới
                            // Check visited without allocating a new dict
                            var newKey = EncodeState(sortedIds, origins, blockId, probe);
                            if (cameFrom.ContainsKey(newKey))
                                continue;

                            // Lưu parent + move, đưa vào frontier
                            // Record parent+move, enqueue
                            var newOrigins = new Dictionary<string, Vector2Int>(origins) { [blockId] = probe };
                            cameFrom[newKey] = (currentKey, new SolverMove(blockId, dir, steps));
                            frontier.Enqueue((newKey, newOrigins));

                            // Kiểm tra ngay nếu state mới là lời giải
                            // Early-exit: check if the new state is already the goal
                            if (IsSolved(targetId, newOrigins, defs, exits, width, height))
                            {
                                solvedKey = newKey;
                                goto Done;
                            }
                        }
                    }
                }
            }

            Done:
            sw.Stop();

            if (solvedKey == null)
            {
                Debug.LogWarning($"[PuzzleSolver] No solution found. Visited {cameFrom.Count} states in {sw.ElapsedMilliseconds} ms.");
                return null;
            }

            // ── Tái tạo đường đi (Reconstruct path) ─────────────────────────────
            var path = new List<SolverMove>();
            var cur = solvedKey;
            while (cameFrom[cur].parent != null)
            {
                path.Add(cameFrom[cur].move);
                cur = cameFrom[cur].parent;
            }
            path.Reverse();

            Debug.Log($"[PuzzleSolver] Solution: {path.Count} moves, {cameFrom.Count} states visited, {sw.ElapsedMilliseconds} ms.");
            return path;
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static bool IsSolved(
            string targetId,
            IReadOnlyDictionary<string, Vector2Int> origins,
            Dictionary<string, BlockDefinition> defs,
            IReadOnlyList<ExitGateDefinition> exits,
            int width, int height)
        {
            var def = defs[targetId];
            var cells = PuzzleFootprintUtility.GetCells(def.shape, def.orientation, origins[targetId]);

            foreach (var exit in exits)
                foreach (var cell in cells)
                    if (TouchesExit(cell, exit, width, height))
                        return true;

            return false;
        }

        private static bool TouchesExit(Vector2Int cell, ExitGateDefinition exit, int width, int height)
        {
            return exit.edge switch
            {
                ExitEdge.Left => cell.x == 0 && exit.ContainsIndex(cell.y),
                ExitEdge.Right => cell.x == width - 1 && exit.ContainsIndex(cell.y),
                ExitEdge.Top => cell.y == height - 1 && exit.ContainsIndex(cell.x),
                ExitEdge.Bottom => cell.y == 0 && exit.ContainsIndex(cell.x),
                _ => false
            };
        }

        /// <summary>
        ///     Tính occupancy của tất cả block (dùng để check va chạm).
        ///     (Builds the full occupied-cell set for all blocks.)
        /// </summary>
        private static HashSet<Vector2Int> BuildOccupancy(
            IReadOnlyDictionary<string, Vector2Int> origins,
            Dictionary<string, BlockDefinition> defs)
        {
            var set = new HashSet<Vector2Int>();
            foreach (var kv in origins)
            {
                var def = defs[kv.Key];
                foreach (var cell in PuzzleFootprintUtility.GetCells(def.shape, def.orientation, kv.Value))
                    set.Add(cell);
            }
            return set;
        }

        private static HashSet<Vector2Int> GetCellSet(BlockDefinition def, Vector2Int origin)
        {
            var result = new HashSet<Vector2Int>(
                PuzzleFootprintUtility.GetCells(def.shape, def.orientation, origin));
            return result;
        }

        /// <summary>
        ///     Kiểm tra vị trí mới <paramref name="newCells" /> có hợp lệ không.
        ///     Ô của block đang di chuyển (<paramref name="movingCells" />) được coi là tự do.
        ///     (Validates a new footprint; the moving block's own original cells are treated as free.)
        /// </summary>
        private static bool IsValidPlacement(
            List<Vector2Int> newCells,
            HashSet<Vector2Int> occupied,
            HashSet<Vector2Int> movingCells,
            int width, int height)
        {
            foreach (var cell in newCells)
            {
                if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                    return false;
                if (occupied.Contains(cell) && !movingCells.Contains(cell))
                    return false;
            }
            return true;
        }

        private static Vector2Int[] GetDirections(MoveAxisRule rule)
        {
            return rule switch
            {
                MoveAxisRule.HorizontalOnly => Horizontal,
                MoveAxisRule.VerticalOnly => Vertical,
                _ => AllDirs
            };
        }

        // Mã hoá state thành string — dùng thứ tự sortedIds để ổn định
        // Encode state to a compact string using the pre-sorted ID order
        private static string EncodeState(
            string[] sortedIds,
            IReadOnlyDictionary<string, Vector2Int> origins)
        {
            var sb = new StringBuilder(sortedIds.Length * 5);
            foreach (var id in sortedIds)
            {
                var o = origins[id];
                sb.Append(o.x).Append(',').Append(o.y).Append('|');
            }
            return sb.ToString();
        }

        // Overload: mã hoá với một block bị ghi đè — tránh cấp phát dict mới khi check visited
        // Overload: encode with one block overridden — avoids allocating a new dict for visited-check
        private static string EncodeState(
            string[] sortedIds,
            IReadOnlyDictionary<string, Vector2Int> origins,
            string overrideId,
            Vector2Int overridePos)
        {
            var sb = new StringBuilder(sortedIds.Length * 5);
            foreach (var id in sortedIds)
            {
                var o = id == overrideId ? overridePos : origins[id];
                sb.Append(o.x).Append(',').Append(o.y).Append('|');
            }
            return sb.ToString();
        }
    }
}


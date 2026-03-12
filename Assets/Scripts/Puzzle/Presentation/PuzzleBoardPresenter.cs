using System;
using System.Collections.Generic;
using GonDraz.Base;
using GonDraz.ObjectPool;
using PrimeTween;
using Puzzle.Core;
using UnityEngine;

namespace Puzzle.Presentation
{

    public class PuzzleBoardPresenter : BaseBehaviour
    {
        [SerializeField] private PuzzleGameController controller;
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private PuzzleBlockView blockPrefab;
        [SerializeField] private Vector2 cellSize = new(120f, 120f);
        [SerializeField] private Vector2 cellSpacing = new(8f, 8f);
        [SerializeField] private bool invertY = true;
        
        [Header("Block Sprites (Hình ảnh khối)")]
        [Tooltip("Sprite cho tất cả các block thường. Hỗ trợ 9-slice (sẽ tự động dùng Image.Type.Sliced nếu sprite có border).")]
        [SerializeField] private Sprite normalBlockSprite;
        [Tooltip("Sprite dành riêng cho target block (khối cần di chuyển ra cửa). Dùng normalBlockSprite nếu để trống.")]
        [SerializeField] private Sprite targetBlockSprite;
        [Tooltip("Sprite overlay tùy chọn (lớp bóng / viền / hiệu ứng) cho block thường.")]
        [SerializeField] private Sprite normalBlockOverlaySprite;
        [Tooltip("Sprite overlay tùy chọn cho target block.")]
        [SerializeField] private Sprite targetBlockOverlaySprite;
        [Tooltip("Màu tint của overlay. Set alpha = 0 nếu không muốn overlay.")]
        [SerializeField] private Color overlayColor = new(1f, 1f, 1f, 0.25f);

        [Header("Exit Gate")]
        [SerializeField] private PuzzleExitView exitIndicatorPrefab;
        [SerializeField] private Color exitIndicatorColor     = new(0.2f, 0.85f, 0.3f, 1f);
        [SerializeField] private float exitIndicatorThickness = 20f;
        [SerializeField] private float exitIndicatorGap       = 6f;

        [Header("Grid (Lưới)")]
        [SerializeField] private bool  showGrid    = true;
        [SerializeField] private PuzzleImageView cellBackgroundPrefab; // optional; falls back to runtime creation
        [SerializeField] private Color cellColorA  = new(0.88f, 0.88f, 0.88f, 1f);
        [SerializeField] private Color cellColorB  = new(0.82f, 0.82f, 0.82f, 1f);
        [SerializeField] private bool  checkerboard = true;

        [Header("Border (Tường)")]
        [SerializeField] private bool  showBorder      = true;
        [SerializeField] private PuzzleImageView borderWallPrefab; // optional; falls back to runtime creation
        [SerializeField] private Color borderColor     = new(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField] private float borderThickness = 16f;

        [Header("Movement Arrows (Mũi tên di chuyển)")]
        [Tooltip("Hiện mũi tên chỉ hướng có thể di chuyển trên mỗi block.")]
        [SerializeField] private bool showMovementArrows = true;
        [Tooltip("Sprite mũi tên — mặc định trỏ LÊN (0°). Null = dùng sprite trắng mặc định.")]
        [SerializeField] private Sprite arrowSprite;
        [SerializeField] private Color arrowColor = Color.white;
        [Tooltip("Kích thước cạnh của mỗi mũi tên (pixels).")]
        [SerializeField] private float arrowSize   = 32f;
        [Tooltip("Khoảng lùi từ cạnh block vào tâm mũi tên (pixels). Mũi tên nằm bên trong block.")]
        [SerializeField] private float arrowOffset = 8f;

        private readonly List<Vector2Int>   _cellBuffer  = new(4);
        private readonly Dictionary<string, PuzzleBlockView> _views = new(StringComparer.Ordinal);
        private readonly List<PuzzleExitView>  _exitViews   = new();
        private readonly List<PuzzleImageView> _gridCells   = new(); // lưới nền
        private readonly List<PuzzleImageView> _borderWalls = new(); // tường viền

        // Centring offset — computed once per board init. boardRoot.pivot must be (0.5, 0.5).
        private Vector2 _boardOriginOffset;
        private float   _totalBoardW;
        private float   _totalBoardH;

        // ── Drag state ────────────────────────────────────────────────────────────────
        private bool      _isSolved;
        private string    _activeBlockId;
        private Vector2   _activeDragStartPos;
        private Vector2Int _activeAxis;
        private float     _activeBackLimit;
        private float     _activeFrontLimit;
        private float     _horizBack, _horizFront;
        private float     _vertBack,  _vertFront;

        // ── BaseBehaviour wiring ──────────────────────────────────────────────────────

        public override bool SubscribeUsingOnEnable()    => true;
        public override bool UnsubscribeUsingOnDisable() => true;

        public override void Subscribe()
        {
            base.Subscribe();
            if (controller == null) return;
            controller.BoardInitialized += RebuildViews;
            controller.BlockMoved       += OnBlockMoved;
            controller.Solved           += OnSolved;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            if (controller == null) return;
            controller.BoardInitialized -= RebuildViews;
            controller.BlockMoved       -= OnBlockMoved;
            controller.Solved           -= OnSolved;
        }

        // ── Drag API (called by PuzzleBlockView) ─────────────────────────────────────

        public void OnBeginDrag(string blockId)
        {
            if (_isSolved || controller == null) return;
            if (!_views.TryGetValue(blockId, out var view)) return;
            if (!controller.Board.TryGetBlock(blockId, out _)) return;

            _activeBlockId      = blockId;
            _activeAxis         = Vector2Int.zero;
            _activeDragStartPos = view.GetComponent<RectTransform>().anchoredPosition;

            var board      = controller.Board;
            var stepsLeft  = board.QueryMaxSteps(blockId, Vector2Int.left);
            var stepsRight = board.QueryMaxSteps(blockId, Vector2Int.right);
            var stepsDown  = board.QueryMaxSteps(blockId, Vector2Int.down);
            var stepsUp    = board.QueryMaxSteps(blockId, Vector2Int.up);

            var unitH = cellSize.x + cellSpacing.x;
            var unitV = cellSize.y + cellSpacing.y;

            _horizBack  = -stepsLeft  * unitH;
            _horizFront =  stepsRight * unitH;
            _vertBack   = -(invertY ? stepsUp   : stepsDown) * unitV;
            _vertFront  =  (invertY ? stepsDown : stepsUp)   * unitV;
        }

        public void PreviewDrag(string blockId, Vector2 delta)
        {
            if (_isSolved || blockId != _activeBlockId) return;
            if (!_views.TryGetValue(blockId, out var view)) return;
            if (!controller.Board.TryGetBlock(blockId, out var block)) return;

            var rect = view.GetComponent<RectTransform>();
            const float threshold = 8f;

            // Determine and LOCK axis on first significant movement.
            // All three rules use the same lock-once mechanism:
            //   HorizontalOnly → always locks horizontal
            //   VerticalOnly   → always locks vertical
            //   Both           → locks to whichever direction is dominant at threshold crossing
            //                    (after that, stays locked — no mid-drag axis switching)
            if (_activeAxis == Vector2Int.zero)
            {
                if (Mathf.Abs(delta.x) < threshold && Mathf.Abs(delta.y) < threshold)
                {
                    rect.anchoredPosition = _activeDragStartPos;
                    return;
                }

                var rule = block.Definition.moveRule;
                bool lockHorizontal;

                if (rule == MoveAxisRule.HorizontalOnly)
                    lockHorizontal = true;
                else if (rule == MoveAxisRule.VerticalOnly)
                    lockHorizontal = false;
                else // Both: pick dominant direction now and lock it
                    lockHorizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);

                if (lockHorizontal)
                {
                    _activeAxis       = Vector2Int.right;
                    _activeBackLimit  = _horizBack;
                    _activeFrontLimit = _horizFront;
                }
                else
                {
                    _activeAxis       = Vector2Int.up;
                    _activeBackLimit  = _vertBack;
                    _activeFrontLimit = _vertFront;
                }
            }

            // Move only along the locked axis — never in both directions simultaneously
            Vector2 newPos;
            if (_activeAxis.x != 0)
                newPos = new Vector2(
                    _activeDragStartPos.x + Mathf.Clamp(delta.x, _activeBackLimit, _activeFrontLimit),
                    _activeDragStartPos.y);
            else
                newPos = new Vector2(
                    _activeDragStartPos.x,
                    _activeDragStartPos.y + Mathf.Clamp(delta.y, _activeBackLimit, _activeFrontLimit));

            rect.anchoredPosition = newPos;
        }

        public void TryApplyDrag(string blockId, Vector2 dragDelta)
        {
            _activeBlockId = null;
            if (controller == null || _isSolved) return;

            // Use the axis locked during preview; fall back to dragDelta dominant
            // direction if the user never moved past the threshold
            var axisX = _activeAxis.x != 0;
            if (_activeAxis == Vector2Int.zero)
                axisX = Mathf.Abs(dragDelta.x) >= Mathf.Abs(dragDelta.y);

            var unitDistance = axisX ? cellSize.x + cellSpacing.x : cellSize.y + cellSpacing.y;
            var rawDistance  = axisX ? dragDelta.x : dragDelta.y;

            if (Mathf.Abs(rawDistance) < unitDistance * 0.35f)
            {
                SnapViewToGrid(blockId);
                return;
            }

            var steps     = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(rawDistance) / unitDistance));
            var direction = axisX
                ? new Vector2Int(Math.Sign(rawDistance), 0)
                : new Vector2Int(0, invertY ? -Math.Sign(rawDistance) : Math.Sign(rawDistance));

            if (!controller.TryMoveBlock(blockId, direction, steps))
                SnapViewToGrid(blockId);
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        private void RebuildViews()
        {
            ClearViews();
            _isSolved = false;
            ComputeBoardOriginOffset();

            // Render order (back → front via child hierarchy):
            BuildGridBackground(); // 1. lưới nền (background cells)
            BuildBorderWalls();    // 2. tường viền (outer frame)
            BuildExitViews();      // 3. cửa ra (exit indicators, above border)

            foreach (var pair in controller.Board.Blocks) // 4. blocks (top layer)
            {
                var view = BorrowBlockView();
                view.Bind(pair.Key, this);
                _views[pair.Key] = view;
                ResizeView(view, pair.Value);
                // Block có hình L → tạo image riêng cho từng ô thay vì một rect bao phủ toàn bộ
                if (pair.Value.Definition.shape == BlockShapeType.Corner2x2MissingOne)
                    SetupCornerBlockView(view, pair.Value);
                RefreshSingleView(pair.Key, animate: false);
            }

            ApplyBlockAppearance(); // tint + sprite cho từng block

            if (showMovementArrows)
                foreach (var pair in _views)
                    SetupAndRefreshArrows(pair.Key, pair.Value);
        }

        // ── Board centering ───────────────────────────────────────────────────────────

        private void ComputeBoardOriginOffset()
        {
            var board   = controller.Board;
            _totalBoardW = board.Width  * cellSize.x + Mathf.Max(0, board.Width  - 1) * cellSpacing.x;
            _totalBoardH = board.Height * cellSize.y + Mathf.Max(0, board.Height - 1) * cellSpacing.y;

            if (boardRoot != null)
                boardRoot.sizeDelta = new Vector2(_totalBoardW, _totalBoardH);

            // With block pivot=(0,1): cell(0,0) top-left = (-totalW/2, +totalH/2) for invertY=true
            _boardOriginOffset = new Vector2(
                -_totalBoardW * 0.5f,
                 invertY ? _totalBoardH * 0.5f : -_totalBoardH * 0.5f
            );
        }

        // ── Block appearance (màu + sprite) ──────────────────────────────────────────

        /// <summary>
        /// Áp dụng màu sắc, sprite body và overlay cho tất cả các block.
        /// Gọi sau khi tất cả view đã được tạo ra (sau RebuildViews).
        /// </summary>
        private void ApplyBlockAppearance()
        {
            var targetId = controller.Board.TargetBlockId;
            foreach (var pair in _views)
            {
                var isTarget = pair.Key == targetId;
                pair.Value.SetSprite(isTarget && targetBlockSprite != null ? targetBlockSprite : normalBlockSprite);

                var overlaySprite = isTarget ? targetBlockOverlaySprite : normalBlockOverlaySprite;
                pair.Value.SetOverlaySprite(overlaySprite);
                pair.Value.SetOverlayColor(overlayColor);
            }
        }

        /// <summary>
        /// Dành cho <see cref="BlockShapeType.Corner2x2MissingOne"/>: tính các ô bị chiếm
        /// (relative to bbox-min) rồi giao cho <see cref="PuzzleBlockView.SetupCellImages"/>
        /// tạo Image con cho từng ô, ẩn blockImage bao phủ toàn bộ bounding box.
        /// </summary>
        private void SetupCornerBlockView(PuzzleBlockView view, PuzzleBlockState block)
        {
            // FillCells dùng origin = (0,0) → cells trong không gian cục bộ
            PuzzleFootprintUtility.FillCells(
                block.Definition.shape, block.Definition.orientation,
                Vector2Int.zero, _cellBuffer);

            // Normalize về bbox-min để cell offset đầu tiên luôn là (0,0)
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            foreach (var c in _cellBuffer)
            {
                if (c.x < minX) minX = c.x;
                if (c.y < minY) minY = c.y;
            }

            var normalized = new List<Vector2Int>(_cellBuffer.Count);
            foreach (var c in _cellBuffer)
                normalized.Add(new Vector2Int(c.x - minX, c.y - minY));

            view.SetupCellImages(normalized, cellSize, cellSpacing, invertY);
        }

        // ── Movement arrows ───────────────────────────────────────────────────────────

        /// <summary>
        /// Tạo mới 4 arrow Image cho <paramref name="view"/> rồi cập nhật trạng thái hiện/ẩn
        /// dựa trên số bước khả dụng. Gọi sau khi <c>ResizeView</c> đã set <c>sizeDelta</c>.
        /// </summary>
        private void SetupAndRefreshArrows(string blockId, PuzzleBlockView view)
        {
            view.SetupArrows(arrowSprite, arrowColor, arrowSize, arrowOffset);
            RefreshArrowsForBlock(blockId, view);
        }

        /// <summary>
        /// Cập nhật hiện/ẩn 4 mũi tên của một block theo số bước thực tế trên board.
        /// Chuyển từ board direction sang visual direction dựa trên <c>invertY</c>:
        /// <list type="bullet">
        ///   <item>invertY=true: board +Y (Vector2Int.up) → visual DOWN, board -Y → visual UP</item>
        ///   <item>invertY=false: board +Y → visual UP, board -Y → visual DOWN</item>
        /// </list>
        /// </summary>
        private void RefreshArrowsForBlock(string blockId, PuzzleBlockView view)
        {
            var board = controller.Board;
            var stepsLeft  = board.QueryMaxSteps(blockId, Vector2Int.left);
            var stepsRight = board.QueryMaxSteps(blockId, Vector2Int.right);
            var stepsUp    = board.QueryMaxSteps(blockId, Vector2Int.up);   // board +Y
            var stepsDown  = board.QueryMaxSteps(blockId, Vector2Int.down); // board -Y

            view.UpdateArrows(
                showLeft:  stepsLeft  > 0,
                showRight: stepsRight > 0,
                showUp:    invertY ? stepsDown > 0 : stepsUp   > 0,
                showDown:  invertY ? stepsUp   > 0 : stepsDown > 0
            );
        }

        /// <summary>Cập nhật mũi tên cho toàn bộ block trên board.</summary>
        private void RefreshAllArrows()
        {
            foreach (var pair in _views)
                RefreshArrowsForBlock(pair.Key, pair.Value);
        }

        /// <summary>
        /// Spawns one PuzzleExitView per ExitGateDefinition, positioned just outside
        /// the corresponding board edge.
        ///
        /// Coordinate notes (invertY = true, pivot = (0,1)):
        ///   ExitEdge.Right/Left  → startIndex is a ROW index
        ///   ExitEdge.Top         → startIndex is a COLUMN index; row (Height-1) = visual BOTTOM
        ///   ExitEdge.Bottom      → startIndex is a COLUMN index; row 0        = visual TOP
        /// </summary>
        private void BuildExitViews()
        {
            var cx = cellSize.x;    var cy = cellSize.y;
            var sx = cellSpacing.x; var sy = cellSpacing.y;
            var tw = _totalBoardW;  var th = _totalBoardH;
            var t  = exitIndicatorThickness;
            var g  = exitIndicatorGap;

            foreach (var exit in controller.Board.Exits)
            {
                var view = BorrowExitView();
                var rect = view.GetComponent<RectTransform>();

                // All exit rects use the same anchor/pivot convention as blocks
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot     = new Vector2(0f,   1f);     // top-left origin

                float posX, posY, w, h;

                switch (exit.edge)
                {
                    case ExitEdge.Right:
                        // Sits to the right of the board, spans rows [startIndex … +length)
                        posX = tw * 0.5f + g;
                        posY = ExitRowY(exit.startIndex);
                        w    = t;
                        h    = ExitSpan(exit.length, cy, sy);
                        break;

                    case ExitEdge.Left:
                        // Sits to the left of the board
                        posX = -tw * 0.5f - g - t;
                        posY = ExitRowY(exit.startIndex);
                        w    = t;
                        h    = ExitSpan(exit.length, cy, sy);
                        break;

                    case ExitEdge.Top:
                        // cell.y == Height-1 → with invertY=true this is the visual BOTTOM
                        posX = ExitColX(exit.startIndex);
                        posY = invertY ? (-th * 0.5f - g) : (th * 0.5f + g + t);
                        w    = ExitSpan(exit.length, cx, sx);
                        h    = t;
                        break;

                    case ExitEdge.Bottom:
                        // cell.y == 0 → with invertY=true this is the visual TOP
                        posX = ExitColX(exit.startIndex);
                        posY = invertY ? (th * 0.5f + g + t) : (-th * 0.5f - g);
                        w    = ExitSpan(exit.length, cx, sx);
                        h    = t;
                        break;

                    default: continue;
                }

                rect.anchoredPosition = new Vector2(posX, posY);
                rect.sizeDelta        = new Vector2(w, h);
                view.SetColor(exitIndicatorColor);
                _exitViews.Add(view);
            }
        }

        // Y-coord (top-left, pivot=(0,1)) of the given board ROW — matches block Y
        private float ExitRowY(int rowIndex)
        {
            var y = rowIndex * (cellSize.y + cellSpacing.y);
            return (invertY ? -y : y) + _boardOriginOffset.y;
        }

        // X-coord of the given board COLUMN — matches block X
        private float ExitColX(int colIndex)
            => colIndex * (cellSize.x + cellSpacing.x) + _boardOriginOffset.x;

        // Pixel length of an exit that spans <length> cells
        private static float ExitSpan(int length, float size, float spacing)
            => length * size + Mathf.Max(0, length - 1) * spacing;

        // ── View helpers ──────────────────────────────────────────────────────────────

        private void ResizeView(PuzzleBlockView view, PuzzleBlockState block)
        {
            var rect = view.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0f,   1f);

            PuzzleFootprintUtility.FillCells(block.Definition.shape, block.Definition.orientation,
                Vector2Int.zero, _cellBuffer);

            var minX = int.MaxValue; var minY = int.MaxValue;
            var maxX = int.MinValue; var maxY = int.MinValue;

            foreach (var cell in _cellBuffer)
            {
                minX = Mathf.Min(minX, cell.x); minY = Mathf.Min(minY, cell.y);
                maxX = Mathf.Max(maxX, cell.x); maxY = Mathf.Max(maxY, cell.y);
            }

            var widthCells  = maxX - minX + 1;
            var heightCells = maxY - minY + 1;

            rect.sizeDelta = new Vector2(
                widthCells  * cellSize.x + Mathf.Max(0, widthCells  - 1) * cellSpacing.x,
                heightCells * cellSize.y + Mathf.Max(0, heightCells - 1) * cellSpacing.y
            );
        }

        private void RefreshSingleView(string blockId, bool animate)
        {
            if (!_views.TryGetValue(blockId, out var view)) return;
            if (!controller.Board.TryGetBlock(blockId, out var block)) return;

            var rect = view.GetComponent<RectTransform>();
            if (rect == null) return;

            var target = CellToAnchoredPosition(block.Origin);

            // Skip tween when already at the target position (e.g. live-drag preview left
            // the block exactly on the grid cell). PrimeTween warns if endValue == current.
            if ((rect.anchoredPosition - target).sqrMagnitude < 0.01f)
            {
                rect.anchoredPosition = target;
                return;
            }

            if (animate)
                Tween.UIAnchoredPosition(rect, target, 0.12f, Ease.OutCubic);
            else
                rect.anchoredPosition = target;
        }

        private void SnapViewToGrid(string blockId) => RefreshSingleView(blockId, animate: true);

        private Vector2 CellToAnchoredPosition(Vector2Int cell)
        {
            var x = cell.x * (cellSize.x + cellSpacing.x);
            var y = cell.y * (cellSize.y + cellSpacing.y);
            if (invertY) y = -y;
            return new Vector2(x, y) + _boardOriginOffset;
        }

        private void OnBlockMoved(string blockId, Vector2Int _)
        {
            RefreshSingleView(blockId, animate: true);
            // Sau mỗi di chuyển, cập nhật mũi tên toàn board vì
            // block khác có thể bị chặn/mở theo hướng mới
            if (showMovementArrows)
                RefreshAllArrows();
        }

        private void OnSolved()
        {
            _isSolved = true;
            // Ẩn hết mũi tên khi puzzle đã giải xong
            if (showMovementArrows)
                foreach (var view in _views.Values)
                    view.UpdateArrows(false, false, false, false);
            Debug.Log("[Puzzle] Solved — input disabled.", this);
        }

        // ── Grid background (Lưới) ────────────────────────────────────────────────────

        private void BuildGridBackground()
        {
            if (!showGrid) return;

            var board = controller.Board;
            for (var r = 0; r < board.Height; r++)
            for (var c = 0; c < board.Width; c++)
            {
                var color = (checkerboard && (c + r) % 2 == 1) ? cellColorB : cellColorA;
                var view  = BorrowImageView(cellBackgroundPrefab, $"Cell_{c}_{r}");
                view.SetColor(color);

                var rect = view.GetComponent<RectTransform>();
                rect.anchorMin        = new Vector2(0.5f, 0.5f);
                rect.anchorMax        = new Vector2(0.5f, 0.5f);
                rect.pivot            = new Vector2(0f,   1f);
                rect.anchoredPosition = CellToAnchoredPosition(new Vector2Int(c, r));
                rect.sizeDelta        = cellSize;

                _gridCells.Add(view);
            }
        }

        // ── Border walls (Tường) ──────────────────────────────────────────────────────

        private void BuildBorderWalls()
        {
            if (!showBorder) return;

            var tw = _totalBoardW;
            var th = _totalBoardH;
            var t  = borderThickness;

            SpawnBorderWall("Wall_Top",    new Vector2(-tw * 0.5f - t,  th * 0.5f + t), new Vector2(tw + 2f * t, t));
            SpawnBorderWall("Wall_Bottom", new Vector2(-tw * 0.5f - t, -th * 0.5f),     new Vector2(tw + 2f * t, t));
            SpawnBorderWall("Wall_Left",   new Vector2(-tw * 0.5f - t,  th * 0.5f),     new Vector2(t, th));
            SpawnBorderWall("Wall_Right",  new Vector2( tw * 0.5f,       th * 0.5f),     new Vector2(t, th));
        }

        private void SpawnBorderWall(string wallName, Vector2 pos, Vector2 size)
        {
            var view = BorrowImageView(borderWallPrefab, wallName);
            view.SetColor(borderColor);

            var rect = view.GetComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0f,   1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta        = size;

            _borderWalls.Add(view);
        }

        // ── Pool helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a <see cref="PuzzleBlockView"/> from the pool (prefab must be assigned).
        /// Uses GonDraz <see cref="PoolManager"/> so objects are recycled across board resets.
        /// </summary>
        private PuzzleBlockView BorrowBlockView()
            => PoolManager.GetPool(blockPrefab.gameObject).Get<PuzzleBlockView>(boardRoot);

        /// <summary>
        /// Gets a <see cref="PuzzleExitView"/> from the pool when a prefab is assigned,
        /// or creates a plain runtime rect as fallback (no pool — <see cref="PoolManager.ReturnObject"/>
        /// will Destroy it because it has no <see cref="PoolMember"/>).
        /// </summary>
        private PuzzleExitView BorrowExitView()
        {
            if (exitIndicatorPrefab != null)
                return PoolManager.GetPool(exitIndicatorPrefab.gameObject).Get<PuzzleExitView>(boardRoot);

            // Fallback: procedural rect, no PoolMember → ReturnObject will Destroy it
            var go = new GameObject("ExitIndicator", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(boardRoot, false);
            return go.AddComponent<PuzzleExitView>();
        }

        /// <summary>
        /// Gets a <see cref="PuzzleImageView"/> from the pool when <paramref name="prefab"/> is set,
        /// or creates a plain runtime rect as fallback.
        /// </summary>
        private PuzzleImageView BorrowImageView(PuzzleImageView prefab, string fallbackName)
        {
            if (prefab != null)
                return PoolManager.GetPool(prefab.gameObject).Get<PuzzleImageView>(boardRoot);

            // Fallback: procedural rect, no PoolMember → ReturnObject will Destroy it
            var go = new GameObject(fallbackName, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(boardRoot, false);
            return go.AddComponent<PuzzleImageView>();
        }

        /// <summary>
        /// Returns all active views to their respective pools.
        /// <see cref="PoolManager.ReturnObject"/> handles both pooled objects (has <see cref="PoolMember"/>
        /// → returned to pool) and procedural fallback objects (no PoolMember → Destroyed).
        /// </summary>
        private void ClearViews()
        {
            foreach (var view in _views.Values)
                if (view != null) PoolManager.ReturnObject(view.gameObject);
            _views.Clear();

            foreach (var exitView in _exitViews)
                if (exitView != null) PoolManager.ReturnObject(exitView.gameObject);
            _exitViews.Clear();

            foreach (var cell in _gridCells)
                if (cell != null) PoolManager.ReturnObject(cell.gameObject);
            _gridCells.Clear();

            foreach (var wall in _borderWalls)
                if (wall != null) PoolManager.ReturnObject(wall.gameObject);
            _borderWalls.Clear();
        }
    }
}


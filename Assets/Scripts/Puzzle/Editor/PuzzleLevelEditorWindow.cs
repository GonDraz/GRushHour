#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Puzzle.Core;
using Puzzle.Data;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor
{
    /// <summary>
    /// Visual level editor window for PuzzleLevelSO.
    /// Menu: Tools/Puzzle/Level Editor
    ///
    /// Left panel  — Odin (or default) property inspector for the selected level.
    /// Right panel — interactive IMGUI board canvas:
    ///   • Left-click  block → select / begin drag (snap to grid on mouse-up)
    ///   • Right-click block → context menu (mark target, rotate, delete)
    ///   • Del key          → delete selected block
    ///   • Scroll wheel     → zoom in/out
    ///
    /// Quick-add block presets and exit buttons sit at the bottom of the left panel.
    /// Validation results appear below the presets after clicking "Validate".
    /// </summary>
    public class PuzzleLevelEditorWindow : EditorWindow
    {
        // ── Layout constants ──────────────────────────────────────────────────────────
        private const float LeftPanelW  = 320f;
        private const float TopBarH     = 24f;
        private const float CellPxMin   = 20f;
        private const float CellPxMax   = 110f;
        private const float CellSpacing = 4f;
        private const float ExitThicknessRatio = 0.22f; // fraction of cellPx
        private const float ExitGap     = 5f;

        // ── Canvas colours ────────────────────────────────────────────────────────────
        private static readonly Color ColBg         = new(0.13f, 0.13f, 0.13f, 1f);
        private static readonly Color ColBoardBg    = new(0.20f, 0.20f, 0.20f, 1f);
        private static readonly Color ColCellA      = new(0.24f, 0.24f, 0.24f, 1f);
        private static readonly Color ColCellB      = new(0.20f, 0.20f, 0.20f, 1f);
        private static readonly Color ColBlock      = new(0.28f, 0.62f, 0.78f, 1f);
        private static readonly Color ColTarget     = new(0.92f, 0.38f, 0.32f, 1f);
        private static readonly Color ColSelected   = new(1.00f, 0.85f, 0.18f, 1f);
        private static readonly Color ColGhost      = new(0.65f, 0.65f, 0.65f, 0.30f);
        private static readonly Color ColExit       = new(0.20f, 0.85f, 0.44f, 1f);
        private static readonly Color ColOob        = new(0.85f, 0.18f, 0.18f, 0.40f);
        private static readonly Color ColDivider    = new(0.08f, 0.08f, 0.08f, 1f);
        private static readonly Color ColHiLight    = new(1.00f, 1.00f, 1.00f, 0.10f);

        // ── State ─────────────────────────────────────────────────────────────────────
        [SerializeField] private PuzzleLevelSO _level;
        private float _cellPx = 60f;

        // Canvas interaction
        private string    _selectedId;
        private bool      _isDragging;
        private string    _dragId;
        private Vector2Int _dragOffset;      // cell offset from block origin to click cell
        private Vector2Int _dragCurrentCell; // cell under cursor while dragging

        // Computed per frame
        private Rect   _rightPanelAbsRect;   // absolute rect of the board area in window space
        private Vector2 _boardOffset;         // canvas-local top-left of the board

        // Validation
        private readonly List<string> _validationErrors = new();
        private bool _validationRun;

        // Text preview toggle
        private bool _showTextPreview;

        // Scroll positions
        private Vector2 _leftScroll;

        // Selected exit in the list panel (block selection reuses _selectedId)
        private string _selectedExitId;
        // Temp buffer shared across draw calls (never nested)
        private readonly List<Vector2Int> _cellBuf = new(4);

        // ── Open ─────────────────────────────────────────────────────────────────────

        [MenuItem("Tools/Puzzle/Level Editor")]
        public static void Open() => Open(null);

        public static void Open(PuzzleLevelSO level)
        {
            var win = GetWindow<PuzzleLevelEditorWindow>("Puzzle Level Editor");
            win.minSize = new Vector2(740f, 500f);
            if (level != null) win.LoadLevel(level);
            win.Show();
        }

        // ── Unity callbacks ───────────────────────────────────────────────────────────

        private void OnEnable()
        {
            if (_level != null) RebuildTree();
        }

        private void OnDisable()
        {
            DisposeTree();
        }

        private void OnGUI()
        {
            DrawTopBar();

            if (_level == null)
            {
                var msgRect = new Rect(LeftPanelW + 20, TopBarH + 20, position.width - LeftPanelW - 40, 60);
                EditorGUI.HelpBox(msgRect, "Assign or create a PuzzleLevelSO asset to start editing.", MessageType.Info);
                return;
            }

            var bodyH = position.height - TopBarH;

            // Divider
            EditorGUI.DrawRect(new Rect(LeftPanelW, TopBarH, 1f, bodyH), ColDivider);

            // Left panel
            GUILayout.BeginArea(new Rect(0, TopBarH, LeftPanelW, bodyH));
            DrawLeftPanel();
            GUILayout.EndArea();

            // Right panel — board canvas uses absolute window coords (no BeginArea wrapper,
            // which conflicts with EditorGUI.DrawRect).  Text preview needs GUILayout flow
            // so it keeps its own BeginArea.
            _rightPanelAbsRect = new Rect(LeftPanelW + 1, TopBarH,
                position.width - LeftPanelW - 1, bodyH);

            if (_showTextPreview)
            {
                GUILayout.BeginArea(_rightPanelAbsRect);
                DrawTextPreview(new Rect(0, 0, _rightPanelAbsRect.width, _rightPanelAbsRect.height));
                GUILayout.EndArea();
            }
            else
            {
                // Absolute coordinates — area.x/y are included in _boardOffset inside the method
                DrawBoardCanvas(_rightPanelAbsRect);
            }

            if (GUI.changed && _level != null)
                EditorUtility.SetDirty(_level);
        }

        // ── Top bar ───────────────────────────────────────────────────────────────────

        private void DrawTopBar()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, TopBarH));
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Level:", GUILayout.Width(46));
                var newLevel = (PuzzleLevelSO)EditorGUILayout.ObjectField(
                    _level, typeof(PuzzleLevelSO), false, GUILayout.Width(190));
                if (newLevel != _level) LoadLevel(newLevel);

                GUILayout.Space(10);
                GUILayout.Label("Zoom:", GUILayout.Width(42));
                _cellPx = GUILayout.HorizontalSlider(_cellPx, CellPxMin, CellPxMax, GUILayout.Width(80));
                GUILayout.Label($"{_cellPx:0}px", EditorStyles.miniLabel, GUILayout.Width(34));

                GUILayout.FlexibleSpace();

                if (_level != null)
                {
                    if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(62)))
                        RunValidation();

                    if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(42)))
                        SaveLevel();

                    _showTextPreview = GUILayout.Toggle(
                        _showTextPreview, "Text Preview",
                        EditorStyles.toolbarButton, GUILayout.Width(84));
                }
            }
            GUILayout.EndArea();
        }

        // ── Left panel ────────────────────────────────────────────────────────────────

        private void DrawLeftPanel()
        {
            _leftScroll = GUILayout.BeginScrollView(_leftScroll);
            GUILayout.Space(4);

            DrawBoardSettings();
            GUILayout.Space(6);
            DrawBlocksList();
            GUILayout.Space(6);
            DrawExitsList();
            GUILayout.Space(8);
            DrawBlockPresets();
            DrawExitPresets();

            if (_validationRun)
            {
                GUILayout.Space(6);
                if (_validationErrors.Count == 0)
                    EditorGUILayout.HelpBox("✔  Level is valid.", MessageType.Info);
                else
                {
                    EditorGUILayout.LabelField($"Errors ({_validationErrors.Count})", EditorStyles.boldLabel);
                    foreach (var err in _validationErrors)
                        EditorGUILayout.HelpBox(err, MessageType.Error);
                }
            }

            GUILayout.Space(8);
            GUILayout.EndScrollView();
        }

        // ── Board settings section ────────────────────────────────────────────────────

        private void DrawBoardSettings()
        {
            EditorGUILayout.LabelField("Board", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                var newSize = EditorGUILayout.Vector2IntField("Size", _level.boardSize);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Board Size");
                    _level.boardSize = new Vector2Int(Mathf.Max(1, newSize.x), Mathf.Max(1, newSize.y));
                    EditorUtility.SetDirty(_level);
                    _validationRun = false;
                }

                EditorGUI.BeginChangeCheck();
                var newTarget = EditorGUILayout.TextField("Target Block ID", _level.targetBlockId);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Target Block ID");
                    _level.targetBlockId = newTarget;
                    EditorUtility.SetDirty(_level);
                }
            }
        }

        // ── Blocks list section ───────────────────────────────────────────────────────

        private void DrawBlocksList()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Blocks  ({_level.blocks.Count})", EditorStyles.boldLabel);
                if (GUILayout.Button("+ Block", EditorStyles.miniButton, GUILayout.Width(58)))
                    AddBlock(BlockShapeType.Line1x2, BlockOrientation.Horizontal, MoveAxisRule.HorizontalOnly);
            }

            int removeIdx = -1;
            for (var i = 0; i < _level.blocks.Count; i++)
            {
                var blk = _level.blocks[i];
                if (blk == null) continue;

                var isSel = blk.id == _selectedId;
                DrawBlockRow(blk, isSel, ref removeIdx, i);
                if (isSel) DrawBlockDetail(blk);
            }

            if (removeIdx >= 0)
            {
                var removedId = _level.blocks[removeIdx]?.id;
                Undo.RecordObject(_level, "Delete Block");
                _level.blocks.RemoveAt(removeIdx);
                if (_selectedId == removedId) _selectedId = null;
                _validationRun = false;
                EditorUtility.SetDirty(_level);
                Repaint();
            }
        }

        private void DrawBlockRow(BlockDefinition blk, bool isSel, ref int removeIdx, int idx)
        {
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = isSel
                ? new Color(1.00f, 0.92f, 0.35f)
                : blk.isTarget
                    ? new Color(1.00f, 0.78f, 0.73f)
                    : new Color(0.73f, 0.87f, 1.00f);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUI.backgroundColor = prevBg;

                // Expand / collapse
                if (GUILayout.Button(isSel ? "▾" : "▸",
                    EditorStyles.miniButtonLeft, GUILayout.Width(22)))
                {
                    _selectedId = isSel ? null : blk.id;
                    Repaint();
                }

                // ID — editable inline, confirmed on Enter/focus-loss
                EditorGUI.BeginChangeCheck();
                var newId = EditorGUILayout.DelayedTextField(blk.id, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck()
                    && !string.IsNullOrWhiteSpace(newId)
                    && newId != blk.id
                    && !_level.blocks.Exists(b => b != null && b.id == newId))
                {
                    Undo.RecordObject(_level, "Rename Block");
                    if (_level.targetBlockId == blk.id) _level.targetBlockId = newId;
                    if (_selectedId == blk.id)          _selectedId = newId;
                    blk.id = newId;
                    EditorUtility.SetDirty(_level);
                }

                // Shape + orientation compact label
                EditorGUILayout.LabelField(BlockShapeLabel(blk),
                    EditorStyles.miniLabel, GUILayout.Width(44));

                // Move rule
                EditorGUILayout.LabelField(MoveRuleLabel(blk.moveRule),
                    EditorStyles.miniLabel, GUILayout.Width(20));

                // Target star
                if (blk.isTarget)
                    EditorGUILayout.LabelField("★", EditorStyles.miniLabel, GUILayout.Width(14));

                // Delete
                GUI.color = new Color(1f, 0.55f, 0.55f);
                if (GUILayout.Button("✕", EditorStyles.miniButtonRight, GUILayout.Width(22)))
                    removeIdx = idx;
                GUI.color = Color.white;
            }

            GUI.backgroundColor = prevBg;
        }

        private void DrawBlockDetail(BlockDefinition blk)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // Shape
                EditorGUI.BeginChangeCheck();
                var newShape = (BlockShapeType)EditorGUILayout.EnumPopup("Shape", blk.shape);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Block Shape");
                    blk.shape       = newShape;
                    blk.orientation = DefaultOrientationForShape(newShape);
                    _validationRun  = false;
                    EditorUtility.SetDirty(_level);
                    Repaint();
                }

                // Orientation — filtered list per shape
                var validOrients = ValidOrientationsForShape(blk.shape);
                var orientIdx    = Mathf.Max(0, System.Array.IndexOf(validOrients, blk.orientation));
                EditorGUI.BeginChangeCheck();
                var newOrientIdx = EditorGUILayout.Popup("Orientation", orientIdx,
                    System.Array.ConvertAll(validOrients, OrientationDisplayName));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Block Orientation");
                    blk.orientation = validOrients[newOrientIdx];
                    EditorUtility.SetDirty(_level);
                    Repaint();
                }

                // Move rule
                EditorGUI.BeginChangeCheck();
                var newRule = (MoveAxisRule)EditorGUILayout.EnumPopup("Move Rule", blk.moveRule);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Block Move Rule");
                    blk.moveRule = newRule;
                    EditorUtility.SetDirty(_level);
                }

                // Is target
                EditorGUI.BeginChangeCheck();
                var newIsTarget = EditorGUILayout.Toggle("Is Target", blk.isTarget);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Set Target");
                    if (newIsTarget)
                    {
                        foreach (var b in _level.blocks) if (b != null) b.isTarget = false;
                        blk.isTarget         = true;
                        _level.targetBlockId = blk.id;
                    }
                    else blk.isTarget = false;
                    EditorUtility.SetDirty(_level);
                    Repaint();
                }

                // Position — read-only; drag on canvas to change
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.Vector2IntField("Position (drag on canvas)", blk.startCell);
            }
        }

        // ── Exits list section ────────────────────────────────────────────────────────

        private void DrawExitsList()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Exits  ({_level.exits.Count})", EditorStyles.boldLabel);
                if (GUILayout.Button("+ Exit →", EditorStyles.miniButton, GUILayout.Width(58)))
                    AddExit(ExitEdge.Right);
            }

            int removeIdx = -1;
            for (var i = 0; i < _level.exits.Count; i++)
            {
                var exit = _level.exits[i];
                if (exit == null) continue;

                var isSel = exit.id == _selectedExitId;
                DrawExitRow(exit, isSel, ref removeIdx, i);
                if (isSel) DrawExitDetail(exit);
            }

            if (removeIdx >= 0)
            {
                var removedId = _level.exits[removeIdx]?.id;
                Undo.RecordObject(_level, "Delete Exit");
                _level.exits.RemoveAt(removeIdx);
                if (_selectedExitId == removedId) _selectedExitId = null;
                _validationRun = false;
                EditorUtility.SetDirty(_level);
                Repaint();
            }
        }

        private void DrawExitRow(ExitGateDefinition exit, bool isSel, ref int removeIdx, int idx)
        {
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = isSel
                ? new Color(0.70f, 1.00f, 0.72f)
                : new Color(0.82f, 0.97f, 0.82f);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUI.backgroundColor = prevBg;

                if (GUILayout.Button(isSel ? "▾" : "▸",
                    EditorStyles.miniButtonLeft, GUILayout.Width(22)))
                {
                    _selectedExitId = isSel ? null : exit.id;
                    Repaint();
                }

                // Edge icon
                EditorGUILayout.LabelField(ExitEdgeIcon(exit.edge),
                    EditorStyles.miniLabel, GUILayout.Width(16));

                // ID
                EditorGUI.BeginChangeCheck();
                var newId = EditorGUILayout.DelayedTextField(exit.id, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newId) && newId != exit.id)
                {
                    Undo.RecordObject(_level, "Rename Exit");
                    if (_selectedExitId == exit.id) _selectedExitId = newId;
                    exit.id = newId;
                    EditorUtility.SetDirty(_level);
                }

                // Edge label
                EditorGUILayout.LabelField($"{exit.edge}",
                    EditorStyles.miniLabel, GUILayout.Width(44));

                // Index + length summary
                EditorGUILayout.LabelField($"i:{exit.startIndex}  l:{exit.length}",
                    EditorStyles.miniLabel, GUILayout.Width(52));

                // Delete
                GUI.color = new Color(1f, 0.55f, 0.55f);
                if (GUILayout.Button("✕", EditorStyles.miniButtonRight, GUILayout.Width(22)))
                    removeIdx = idx;
                GUI.color = Color.white;
            }

            GUI.backgroundColor = prevBg;
        }

        private void DrawExitDetail(ExitGateDefinition exit)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // Edge
                EditorGUI.BeginChangeCheck();
                var newEdge = (ExitEdge)EditorGUILayout.EnumPopup("Edge", exit.edge);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Exit Edge");
                    exit.edge      = newEdge;
                    _validationRun = false;
                    EditorUtility.SetDirty(_level);
                    Repaint();
                }

                // Start index
                EditorGUI.BeginChangeCheck();
                var newStart = EditorGUILayout.IntField("Start Index", exit.startIndex);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Exit Start Index");
                    exit.startIndex = Mathf.Max(0, newStart);
                    _validationRun  = false;
                    EditorUtility.SetDirty(_level);
                }

                // Length
                EditorGUI.BeginChangeCheck();
                var newLen = EditorGUILayout.IntField("Length", exit.length);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Exit Length");
                    exit.length    = Mathf.Max(1, newLen);
                    _validationRun = false;
                    EditorUtility.SetDirty(_level);
                }

                // Only target
                EditorGUI.BeginChangeCheck();
                var newOnly = EditorGUILayout.Toggle("Only Target", exit.onlyTarget);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Exit Only Target");
                    exit.onlyTarget = newOnly;
                    EditorUtility.SetDirty(_level);
                }

                var hint = (exit.edge == ExitEdge.Left || exit.edge == ExitEdge.Right)
                    ? "Start Index = row  (0 = top of board)"
                    : "Start Index = column  (0 = left)";
                EditorGUILayout.HelpBox(hint, MessageType.None);
            }
        }

        // ── Quick-add presets ─────────────────────────────────────────────────────────

        private void DrawBlockPresets()
        {
            GUILayout.Label("Quick Add Block", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("1×1"))   AddBlock(BlockShapeType.Single1x1,         BlockOrientation.Horizontal,         MoveAxisRule.Both);
                if (GUILayout.Button("1×2 →")) AddBlock(BlockShapeType.Line1x2,            BlockOrientation.Horizontal,         MoveAxisRule.HorizontalOnly);
                if (GUILayout.Button("1×2 ↓")) AddBlock(BlockShapeType.Line1x2,            BlockOrientation.Vertical,           MoveAxisRule.VerticalOnly);
                if (GUILayout.Button("1×3 →")) AddBlock(BlockShapeType.Line1x3,            BlockOrientation.Horizontal,         MoveAxisRule.HorizontalOnly);
                if (GUILayout.Button("1×3 ↓")) AddBlock(BlockShapeType.Line1x3,            BlockOrientation.Vertical,           MoveAxisRule.VerticalOnly);
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("2×2"))   AddBlock(BlockShapeType.Square2x2,          BlockOrientation.Horizontal,         MoveAxisRule.Both);
                if (GUILayout.Button("L ↖"))   AddBlock(BlockShapeType.Corner2x2MissingOne, BlockOrientation.MissingTopLeft,    MoveAxisRule.Both);
                if (GUILayout.Button("L ↗"))   AddBlock(BlockShapeType.Corner2x2MissingOne, BlockOrientation.MissingTopRight,   MoveAxisRule.Both);
                if (GUILayout.Button("L ↙"))   AddBlock(BlockShapeType.Corner2x2MissingOne, BlockOrientation.MissingBottomLeft,  MoveAxisRule.Both);
                if (GUILayout.Button("L ↘"))   AddBlock(BlockShapeType.Corner2x2MissingOne, BlockOrientation.MissingBottomRight, MoveAxisRule.Both);
            }
        }

        private void DrawExitPresets()
        {
            GUILayout.Space(4);
            GUILayout.Label("Quick Add Exit", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("← Left"))   AddExit(ExitEdge.Left);
                if (GUILayout.Button("→ Right"))  AddExit(ExitEdge.Right);
                if (GUILayout.Button("↑ Top"))    AddExit(ExitEdge.Top);
                if (GUILayout.Button("↓ Bottom")) AddExit(ExitEdge.Bottom);
            }
        }

        // ── Left-panel label helpers ──────────────────────────────────────────────────

        private static string BlockShapeLabel(BlockDefinition blk) =>
            blk.shape switch
            {
                BlockShapeType.Single1x1           => "1×1",
                BlockShapeType.Line1x2             => blk.orientation == BlockOrientation.Vertical ? "1×2 ↓" : "1×2 →",
                BlockShapeType.Line1x3             => blk.orientation == BlockOrientation.Vertical ? "1×3 ↓" : "1×3 →",
                BlockShapeType.Square2x2           => "2×2",
                BlockShapeType.Corner2x2MissingOne => blk.orientation switch
                {
                    BlockOrientation.MissingTopLeft     => "L ↖",
                    BlockOrientation.MissingTopRight    => "L ↗",
                    BlockOrientation.MissingBottomLeft  => "L ↙",
                    BlockOrientation.MissingBottomRight => "L ↘",
                    _                                   => "L"
                },
                _ => "?"
            };

        private static string MoveRuleLabel(MoveAxisRule rule) =>
            rule switch
            {
                MoveAxisRule.HorizontalOnly => "H",
                MoveAxisRule.VerticalOnly   => "V",
                MoveAxisRule.Both           => "HV",
                _                           => "?"
            };

        private static string ExitEdgeIcon(ExitEdge edge) =>
            edge switch
            {
                ExitEdge.Left   => "←",
                ExitEdge.Right  => "→",
                ExitEdge.Top    => "↑",
                ExitEdge.Bottom => "↓",
                _               => "?"
            };

        private static BlockOrientation[] ValidOrientationsForShape(BlockShapeType shape) =>
            shape switch
            {
                BlockShapeType.Single1x1           => new[] { BlockOrientation.Horizontal },
                BlockShapeType.Line1x2             => new[] { BlockOrientation.Horizontal, BlockOrientation.Vertical },
                BlockShapeType.Line1x3             => new[] { BlockOrientation.Horizontal, BlockOrientation.Vertical },
                BlockShapeType.Square2x2           => new[] { BlockOrientation.Horizontal },
                BlockShapeType.Corner2x2MissingOne => new[]
                {
                    BlockOrientation.MissingTopLeft, BlockOrientation.MissingTopRight,
                    BlockOrientation.MissingBottomLeft, BlockOrientation.MissingBottomRight
                },
                _ => new[] { BlockOrientation.Horizontal }
            };

        private static BlockOrientation DefaultOrientationForShape(BlockShapeType shape) =>
            shape == BlockShapeType.Corner2x2MissingOne
                ? BlockOrientation.MissingTopLeft
                : BlockOrientation.Horizontal;

        private static string OrientationDisplayName(BlockOrientation o) =>
            o switch
            {
                BlockOrientation.Horizontal          => "Horizontal →",
                BlockOrientation.Vertical            => "Vertical ↓",
                BlockOrientation.MissingTopLeft      => "Missing Top-Left ↖",
                BlockOrientation.MissingTopRight     => "Missing Top-Right ↗",
                BlockOrientation.MissingBottomLeft   => "Missing Bottom-Left ↙",
                BlockOrientation.MissingBottomRight  => "Missing Bottom-Right ↘",
                _                                    => o.ToString()
            };

        // ── Text preview ──────────────────────────────────────────────────────────────

        private void DrawTextPreview(Rect area)
        {
            GUILayout.BeginArea(area);
            GUILayout.Label("Text Board Preview  (T = target · B = block · . = empty)", EditorStyles.boldLabel);

            var sb = new StringBuilder();
            var W  = _level.boardSize.x;
            var H  = _level.boardSize.y;

            // Build occupancy (row 0 = top, invertY=true)
            var cells = new Dictionary<Vector2Int, string>();
            foreach (var blk in _level.blocks)
            {
                if (blk == null) continue;
                PuzzleFootprintUtility.FillCells(blk.shape, blk.orientation, blk.startCell, _cellBuf);
                foreach (var c in _cellBuf)
                    cells[c] = blk.isTarget ? "T" : "B";
            }

            for (var row = 0; row < H; row++)
            {
                for (var col = 0; col < W; col++)
                {
                    sb.Append(cells.TryGetValue(new Vector2Int(col, row), out var ch) ? ch : ".");
                    if (col < W - 1) sb.Append(' ');
                }
                if (row < H - 1) sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Exits:");
            foreach (var exit in _level.exits)
            {
                if (exit == null) continue;
                var indexLabel = (exit.edge == ExitEdge.Left || exit.edge == ExitEdge.Right)
                    ? $"row {exit.startIndex}"
                    : $"col {exit.startIndex}";
                sb.AppendLine($"  [{exit.id}]  {exit.edge}  {indexLabel}  len={exit.length}" +
                              (exit.onlyTarget ? "  [targetOnly]" : ""));
            }

            var style = new GUIStyle(EditorStyles.textArea)
                { font = EditorStyles.standardFont, wordWrap = false, fontSize = 13 };
            GUILayout.TextArea(sb.ToString(), style, GUILayout.ExpandHeight(true));
            GUILayout.EndArea();
        }

        // ── Board canvas ──────────────────────────────────────────────────────────────

        private void DrawBoardCanvas(Rect area)
        {
            var W    = _level.boardSize.x;
            var H    = _level.boardSize.y;
            var step = _cellPx + CellSpacing;

            // Board pixel size
            var boardW = W * _cellPx + Mathf.Max(0, W - 1) * CellSpacing;
            var boardH = H * _cellPx + Mathf.Max(0, H - 1) * CellSpacing;

            // Exit indicator thickness
            var exitThick = Mathf.Max(6f, _cellPx * ExitThicknessRatio);
            var margin    = exitThick + ExitGap + 10f;

            // Center board inside canvas — _boardOffset is in absolute window coordinates
            _boardOffset = new Vector2(
                area.x + margin + Mathf.Max(0f, (area.width  - margin * 2f - boardW) * 0.5f),
                area.y + margin + Mathf.Max(0f, (area.height - margin * 2f - boardH) * 0.5f));

            // Background
            EditorGUI.DrawRect(area, ColBg);
            EditorGUI.DrawRect(new Rect(_boardOffset.x, _boardOffset.y, boardW, boardH), ColBoardBg);

            // Grid cells (checkerboard)
            for (var r = 0; r < H; r++)
            for (var c = 0; c < W; c++)
                EditorGUI.DrawRect(CellRect(c, r), (c + r) % 2 == 0 ? ColCellA : ColCellB);

            // Exits
            DrawExits(exitThick, step, boardW, boardH);

            // Ghost (drag preview at drop position)
            if (_isDragging && _dragId != null)
                DrawGhost();

            // Blocks — skip dragged block so it's drawn on top
            foreach (var blk in _level.blocks)
            {
                if (blk == null || (_isDragging && blk.id == _dragId)) continue;
                DrawBlock(blk, blk.id == _selectedId);
            }

            // Dragged block on top
            if (_isDragging && _dragId != null)
            {
                var dragBlk = _level.blocks.Find(b => b != null && b.id == _dragId);
                if (dragBlk != null) DrawBlock(dragBlk, true);
            }

            HandleCanvasInput(area);

            // Cursor coordinate label (absolute window coords)
            var mp = Event.current.mousePosition;
            if (area.Contains(mp))
            {
                var hc = PixelToCell(mp);
                if (hc.x >= 0 && hc.x < W && hc.y >= 0 && hc.y < H)
                    GUI.Label(new Rect(area.x + 4, area.y + area.height - 18, 120, 18),
                        $"col {hc.x}  row {hc.y}",
                        new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } });
            }

            // Selection info bar
            DrawSelectionBar(area);
        }

        private void DrawExits(float thick, float step, float boardW, float boardH)
        {
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal    = { textColor = ColExit },
                alignment = TextAnchor.MiddleCenter,
                fontSize  = Mathf.Clamp((int)(_cellPx * 0.18f), 7, 11)
            };

            foreach (var exit in _level.exits)
            {
                if (exit == null) continue;

                float startOffset, span;
                Rect  exitRect;

                switch (exit.edge)
                {
                    // Left/Right: startIndex = ROW, row 0 = visual top
                    case ExitEdge.Right:
                        startOffset = exit.startIndex * step;
                        span        = ExitSpan(exit.length);
                        exitRect    = new Rect(
                            _boardOffset.x + boardW + ExitGap,
                            _boardOffset.y + startOffset,
                            thick, span);
                        break;

                    case ExitEdge.Left:
                        startOffset = exit.startIndex * step;
                        span        = ExitSpan(exit.length);
                        exitRect    = new Rect(
                            _boardOffset.x - ExitGap - thick,
                            _boardOffset.y + startOffset,
                            thick, span);
                        break;

                    // Top/Bottom: startIndex = COLUMN
                    // ExitEdge.Bottom → cell y=0 → invertY → visual TOP of canvas → ABOVE board
                    case ExitEdge.Bottom:
                        startOffset = exit.startIndex * step;
                        span        = ExitSpan(exit.length);
                        exitRect    = new Rect(
                            _boardOffset.x + startOffset,
                            _boardOffset.y - ExitGap - thick,
                            span, thick);
                        break;

                    // ExitEdge.Top → cell y=H-1 → invertY → visual BOTTOM of canvas → BELOW board
                    case ExitEdge.Top:
                        startOffset = exit.startIndex * step;
                        span        = ExitSpan(exit.length);
                        exitRect    = new Rect(
                            _boardOffset.x + startOffset,
                            _boardOffset.y + boardH + ExitGap,
                            span, thick);
                        break;

                    default: continue;
                }

                EditorGUI.DrawRect(exitRect, ColExit);

                // Expand label rect slightly so text doesn't clip
                var lrect = new Rect(exitRect.x - 2, exitRect.y, exitRect.width + 4, exitRect.height);
                GUI.Label(lrect, exit.id, labelStyle);
            }
        }

        private float ExitSpan(int length)
            => length * _cellPx + Mathf.Max(0, length - 1) * CellSpacing;

        private void DrawBlock(BlockDefinition blk, bool selected)
        {
            // Use drag position when this block is being dragged
            var origin = (_isDragging && blk.id == _dragId)
                ? _dragCurrentCell - _dragOffset
                : blk.startCell;

            PuzzleFootprintUtility.FillCells(blk.shape, blk.orientation, origin, _cellBuf);

            var baseCol = selected ? ColSelected : (blk.isTarget ? ColTarget : ColBlock);

            foreach (var cell in _cellBuf)
            {
                var cr    = CellRect(cell.x, cell.y);
                var isOob = cell.x < 0 || cell.x >= _level.boardSize.x ||
                            cell.y < 0 || cell.y >= _level.boardSize.y;

                EditorGUI.DrawRect(cr, isOob ? ColOob : baseCol);

                if (!isOob)
                {
                    // Top & left inner highlight (1px shimmer)
                    EditorGUI.DrawRect(new Rect(cr.x,      cr.y,      cr.width, 1f),  ColHiLight);
                    EditorGUI.DrawRect(new Rect(cr.x,      cr.y,      1f,  cr.height), ColHiLight);
                }
            }

            // ID label on the origin cell
            if (_cellBuf.Count > 0)
            {
                var labelCell = _cellBuf[0]; // first cell after FillCells = origin
                var lr        = CellRect(labelCell.x, labelCell.y);
                var labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize  = Mathf.Clamp((int)(_cellPx * 0.26f), 8, 18),
                    normal    = { textColor = new Color(0f, 0f, 0f, 0.75f) }
                };
                GUI.Label(lr, blk.isTarget ? "T" : blk.id, labelStyle);
            }
        }

        private void DrawGhost()
        {
            var dragBlk = _level.blocks.Find(b => b != null && b.id == _dragId);
            if (dragBlk == null) return;

            var ghostOrigin = _dragCurrentCell - _dragOffset;
            PuzzleFootprintUtility.FillCells(dragBlk.shape, dragBlk.orientation, ghostOrigin, _cellBuf);
            foreach (var cell in _cellBuf)
                EditorGUI.DrawRect(CellRect(cell.x, cell.y), ColGhost);
        }

        private void DrawSelectionBar(Rect area)
        {
            if (_selectedId == null) return;
            var blk = _level.blocks.Find(b => b != null && b.id == _selectedId);
            if (blk == null) return;

            var style = new GUIStyle(EditorStyles.miniLabel)
                { normal = { textColor = ColSelected } };
            GUI.Label(
                new Rect(area.x + 6, area.y + area.height - 36, area.width - 12, 18),
                $"Selected: [{blk.id}]  {blk.shape} / {blk.orientation}  {blk.moveRule}  origin ({blk.startCell.x},{blk.startCell.y})  —  [Del] delete  [Right-click] options",
                style);
        }

        // ── Canvas input ──────────────────────────────────────────────────────────────

        private void HandleCanvasInput(Rect area)
        {
            var e      = Event.current;
            var mp     = e.mousePosition; // absolute window coords — no BeginArea group active
            var inArea = area.Contains(mp);

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0 && inArea:
                    OnLeftMouseDown(PixelToCell(mp));
                    break;

                case EventType.MouseDrag when e.button == 0 && _isDragging:
                    _dragCurrentCell = PixelToCell(mp);
                    e.Use();
                    Repaint();
                    break;

                case EventType.MouseUp when e.button == 0 && _isDragging:
                    CommitDrag();
                    break;

                case EventType.MouseDown when e.button == 1 && inArea:
                    OpenContextMenu(PixelToCell(mp));
                    break;

                case EventType.KeyDown when e.keyCode == KeyCode.Delete && _selectedId != null:
                    DeleteBlock(_selectedId);
                    e.Use();
                    break;

                case EventType.ScrollWheel when inArea:
                    _cellPx = Mathf.Clamp(_cellPx - e.delta.y * 3f, CellPxMin, CellPxMax);
                    e.Use();
                    Repaint();
                    break;

                case EventType.MouseDown when e.button == 0 && !inArea && _isDragging:
                    _isDragging = false;
                    _dragId     = null;
                    Repaint();
                    break;
            }
        }

        private void OnLeftMouseDown(Vector2Int cell)
        {
            var blk = BlockAtCell(cell);
            if (blk != null)
            {
                _selectedId       = blk.id;
                _isDragging       = true;
                _dragId           = blk.id;
                _dragCurrentCell  = cell;
                _dragOffset       = cell - blk.startCell;
                GUI.FocusControl(null); // allow Del key to fire
                Event.current.Use();
            }
            else
            {
                _selectedId = null;
            }

            Repaint();
        }

        private void CommitDrag()
        {
            _isDragging = false;
            if (_dragId == null) return;

            var blk = _level.blocks.Find(b => b != null && b.id == _dragId);
            if (blk != null)
            {
                var newOrigin = _dragCurrentCell - _dragOffset;
                if (newOrigin != blk.startCell)
                {
                    Undo.RecordObject(_level, $"Move '{blk.id}'");
                    blk.startCell = newOrigin;
                    EditorUtility.SetDirty(_level);
                    _validationRun = false;
                    RefreshTree();
                }
            }

            _dragId = null;
            Repaint();
        }

        private void OpenContextMenu(Vector2Int cell)
        {
            var blk = BlockAtCell(cell);
            if (blk == null) return;

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Select"), false, () =>
            {
                _selectedId = blk.id;
                Repaint();
            });

            menu.AddSeparator("");

            var isTarget = blk.isTarget;
            menu.AddItem(new GUIContent("Mark as Target"), isTarget, () =>
            {
                if (isTarget) return;
                Undo.RecordObject(_level, "Set Target");
                foreach (var b in _level.blocks) if (b != null) b.isTarget = false;
                blk.isTarget       = true;
                _level.targetBlockId = blk.id;
                EditorUtility.SetDirty(_level);
                RefreshTree();
                Repaint();
            });

            menu.AddSeparator("");

            if (blk.shape == BlockShapeType.Corner2x2MissingOne)
            {
                menu.AddItem(new GUIContent("Rotate Corner CW"), false, () =>
                {
                    Undo.RecordObject(_level, "Rotate Corner");
                    blk.orientation = blk.orientation switch
                    {
                        BlockOrientation.MissingTopLeft     => BlockOrientation.MissingTopRight,
                        BlockOrientation.MissingTopRight    => BlockOrientation.MissingBottomRight,
                        BlockOrientation.MissingBottomRight => BlockOrientation.MissingBottomLeft,
                        BlockOrientation.MissingBottomLeft  => BlockOrientation.MissingTopLeft,
                        _                                   => blk.orientation
                    };
                    EditorUtility.SetDirty(_level);
                    RefreshTree();
                    Repaint();
                });
            }
            else if (blk.shape == BlockShapeType.Line1x2 || blk.shape == BlockShapeType.Line1x3)
            {
                var isHoriz = blk.orientation == BlockOrientation.Horizontal;
                menu.AddItem(new GUIContent(isHoriz ? "Flip to Vertical" : "Flip to Horizontal"), false, () =>
                {
                    Undo.RecordObject(_level, "Flip Orientation");
                    blk.orientation = isHoriz ? BlockOrientation.Vertical : BlockOrientation.Horizontal;
                    blk.moveRule    = isHoriz ? MoveAxisRule.VerticalOnly : MoveAxisRule.HorizontalOnly;
                    EditorUtility.SetDirty(_level);
                    RefreshTree();
                    Repaint();
                });
            }

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Move Rule: Horizontal Only"),
                blk.moveRule == MoveAxisRule.HorizontalOnly, () => SetMoveRule(blk, MoveAxisRule.HorizontalOnly));
            menu.AddItem(new GUIContent("Move Rule: Vertical Only"),
                blk.moveRule == MoveAxisRule.VerticalOnly, () => SetMoveRule(blk, MoveAxisRule.VerticalOnly));
            menu.AddItem(new GUIContent("Move Rule: Both"),
                blk.moveRule == MoveAxisRule.Both, () => SetMoveRule(blk, MoveAxisRule.Both));

            menu.AddSeparator("");

            menu.AddItem(new GUIContent($"Delete  '{blk.id}'"), false, () => DeleteBlock(blk.id));

            menu.ShowAsContext();
            Event.current.Use();
        }

        private void SetMoveRule(BlockDefinition blk, MoveAxisRule rule)
        {
            Undo.RecordObject(_level, "Set Move Rule");
            blk.moveRule = rule;
            EditorUtility.SetDirty(_level);
            RefreshTree();
            Repaint();
        }

        // ── Coordinate helpers ────────────────────────────────────────────────────────


        private Rect CellRect(int col, int row)
        {
            var step = _cellPx + CellSpacing;
            return new Rect(_boardOffset.x + col * step, _boardOffset.y + row * step, _cellPx, _cellPx);
        }

        private Vector2Int PixelToCell(Vector2 localPos)
        {
            var step = _cellPx + CellSpacing;
            return new Vector2Int(
                Mathf.FloorToInt((localPos.x - _boardOffset.x) / step),
                Mathf.FloorToInt((localPos.y - _boardOffset.y) / step));
        }

        private BlockDefinition BlockAtCell(Vector2Int cell)
        {
            // Iterate in reverse → topmost (last-drawn) block takes priority
            for (var i = _level.blocks.Count - 1; i >= 0; i--)
            {
                var blk = _level.blocks[i];
                if (blk == null) continue;
                PuzzleFootprintUtility.FillCells(blk.shape, blk.orientation, blk.startCell, _cellBuf);
                if (_cellBuf.Contains(cell)) return blk;
            }
            return null;
        }

        // ── Actions ───────────────────────────────────────────────────────────────────

        private void AddBlock(BlockShapeType shape, BlockOrientation orientation, MoveAxisRule moveRule)
        {
            Undo.RecordObject(_level, "Add Block");
            var id = GenerateBlockId(shape);
            _level.blocks.Add(new BlockDefinition
            {
                id          = id,
                isTarget    = false,
                shape       = shape,
                orientation = orientation,
                moveRule    = moveRule,
                startCell   = FindFreeOrigin(shape, orientation)
            });
            _selectedId    = id;
            _validationRun = false;
            EditorUtility.SetDirty(_level);
            RebuildTree();
            Repaint();
        }

        private void AddExit(ExitEdge edge)
        {
            Undo.RecordObject(_level, "Add Exit");
            var suffix = _level.exits.Count + 1;
            _level.exits.Add(new ExitGateDefinition
            {
                id         = $"exit_{edge.ToString().ToLower()}_{suffix}",
                edge       = edge,
                startIndex = 0,
                length     = 1,
                onlyTarget = true
            });
            _validationRun = false;
            EditorUtility.SetDirty(_level);
            RebuildTree();
            Repaint();
        }

        private void DeleteBlock(string blockId)
        {
            if (blockId == null) return;
            var blk = _level.blocks.Find(b => b != null && b.id == blockId);
            if (blk == null) return;

            Undo.RecordObject(_level, $"Delete '{blockId}'");
            _level.blocks.Remove(blk);
            if (_selectedId == blockId) _selectedId = null;
            _validationRun = false;
            EditorUtility.SetDirty(_level);
            RebuildTree();
            Repaint();
        }

        private string GenerateBlockId(BlockShapeType shape)
        {
            var prefix = shape switch
            {
                BlockShapeType.Single1x1          => "s",
                BlockShapeType.Line1x2            => "b",
                BlockShapeType.Line1x3            => "l",
                BlockShapeType.Square2x2          => "sq",
                BlockShapeType.Corner2x2MissingOne => "c",
                _                                 => "blk"
            };
            var i = 1;
            while (_level.blocks.Exists(b => b != null && b.id == prefix + i)) i++;
            return prefix + i;
        }

        /// <summary>Finds the first board position where the block fits without overlapping others.</summary>
        private Vector2Int FindFreeOrigin(BlockShapeType shape, BlockOrientation orientation)
        {
            var occupied = new HashSet<Vector2Int>();
            foreach (var b in _level.blocks)
            {
                if (b == null) continue;
                PuzzleFootprintUtility.FillCells(b.shape, b.orientation, b.startCell, _cellBuf);
                foreach (var c in _cellBuf) occupied.Add(c);
            }

            var W = Mathf.Max(1, _level.boardSize.x);
            var H = Mathf.Max(1, _level.boardSize.y);

            for (var row = 0; row < H; row++)
            for (var col = 0; col < W; col++)
            {
                var origin = new Vector2Int(col, row);
                PuzzleFootprintUtility.FillCells(shape, orientation, origin, _cellBuf);
                var fits = true;
                foreach (var c in _cellBuf)
                {
                    if (c.x < 0 || c.x >= W || c.y < 0 || c.y >= H || occupied.Contains(c))
                    {
                        fits = false;
                        break;
                    }
                }
                if (fits) return origin;
            }

            return Vector2Int.zero; // fallback
        }

        // ── Validation / Save ─────────────────────────────────────────────────────────

        private void RunValidation()
        {
            PuzzleLevelValidationUtility.Validate(_level, _validationErrors);
            _validationRun = true;
            Repaint();
        }

        private void SaveLevel()
        {
            if (_level == null) return;
            EditorUtility.SetDirty(_level);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PuzzleEditor] Saved: {AssetDatabase.GetAssetPath(_level)}", _level);
        }

        // ── Property tree helpers ─────────────────────────────────────────────────────

        private void LoadLevel(PuzzleLevelSO level)
        {
            _level         = level;
            _selectedId    = null;
            _validationRun = false;
            _validationErrors.Clear();
            RebuildTree();
            Repaint();
        }

        private void RebuildTree() => Repaint();
        private void RefreshTree() { }
        private void DisposeTree() { }
    }
}
#endif









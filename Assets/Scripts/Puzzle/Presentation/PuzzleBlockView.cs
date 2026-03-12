using System;
using System.Collections.Generic;
using GonDraz.ObjectPool;
using PrimeTween;
using Puzzle.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Puzzle.Presentation
{
    // IDragHandler added to mirror StoneMovement.OnMouseDrag: live visual while dragging
    public class PuzzleBlockView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPoolable
    {
        // Assign the main body Image in the prefab; auto-found in Bind() if left empty
        [SerializeField] private Image blockImage;

        // Optional second Image layer (e.g. shine/overlay/border).
        // Leave null in the prefab if not needed.
        [SerializeField] private Image overlayImage;

        // Root chứa cell images cho Corner shape — gán trong prefab.
        // Nên là RectTransform con, stretch-fill, pivot (0,1) để khớp hệ tọa độ block.
        // Nếu null, fallback về transform gốc của block.
        [SerializeField] private RectTransform cellImagesRoot;

        // ── Cell-image mode (dùng cho Corner2x2MissingOne) ───────────────────────────
        private readonly List<Image> _cellImages        = new();
        private readonly List<Image> _cellOverlayImages = new();

        // ── Arrow indicators ──────────────────────────────────────────────────────────
        // Root tự tạo lần đầu (lazy); arrows spawn dưới đây thay vì trực tiếp trên block.
        private RectTransform _arrowsRoot;
        private RectTransform ArrowsRoot
        {
            get
            {
                if (_arrowsRoot != null) return _arrowsRoot;
                var go = new GameObject("ArrowsRoot", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                // Stretch to fill — cùng không gian tọa độ với block root
                var r = go.GetComponent<RectTransform>();
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.pivot     = new Vector2(0f, 1f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
                _arrowsRoot = r;
                return _arrowsRoot;
            }
        }

        private Image _arrowLeft, _arrowRight, _arrowUp, _arrowDown;

        private Vector2 _dragStart;
        private PuzzleBoardPresenter _presenter;
        // Cache canvas để chuyển đổi delta từ screen-pixel sang canvas-pixel.
        // Cached canvas reference used to convert drag delta from screen pixels to canvas pixels.
        private Canvas _canvas;

        public string BlockId { get; private set; }

        // ── Drag ─────────────────────────────────────────────────────────────────────

        // Chuyển delta screen-pixel sang canvas-pixel bằng cách chia cho scaleFactor của Canvas.
        // Converts a screen-pixel drag delta to canvas-pixel space by dividing by the Canvas scaleFactor.
        // Cần thiết vì PointerEventData.position là tọa độ màn hình, còn anchoredPosition là tọa độ canvas.
        // Necessary because PointerEventData.position is in screen space while anchoredPosition is in canvas space.
        private Vector2 ToCanvasDelta(Vector2 screenDelta)
        {
            var sf = (_canvas != null) ? _canvas.scaleFactor : 1f;
            return sf > 0f ? screenDelta / sf : screenDelta;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragStart = eventData.position;
            _presenter?.OnBeginDrag(BlockId);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_presenter == null || string.IsNullOrEmpty(BlockId)) return;
            _presenter.PreviewDrag(BlockId, ToCanvasDelta(eventData.position - _dragStart));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_presenter == null || string.IsNullOrEmpty(BlockId)) return;
            _presenter.TryApplyDrag(BlockId, ToCanvasDelta(eventData.position - _dragStart));
        }

        // ── Bind ─────────────────────────────────────────────────────────────────────

        public void Bind(string blockId, PuzzleBoardPresenter presenter)
        {
            BlockId = blockId;
            _presenter = presenter;
            _canvas = GetComponentInParent<Canvas>();
            gameObject.name = $"Block_{blockId}";

            if (blockImage == null)
                blockImage = GetComponentInChildren<Image>(true);
        }

        // ── Appearance setters ────────────────────────────────────────────────────────

        /// <summary>Tints the block's body Image(s).</summary>
        public void SetColor(Color color) =>
            ApplyToBodyImages(img => img.color = color);

        /// <summary>
        /// Sets the sprite on the body Image(s).
        /// Tự động dùng <see cref="Image.Type.Sliced"/> nếu sprite có 9-slice border.
        /// Pass <c>null</c> để revert về solid-colour rect.
        /// </summary>
        public void SetSprite(Sprite sprite) =>
            ApplyToBodyImages(img => ApplySpriteToImage(img, sprite));

        /// <summary>Tints the optional overlay Image(s).</summary>
        public void SetOverlayColor(Color color) =>
            ApplyToOverlayImages(img => img.color = color);

        /// <summary>Sets the sprite on the optional overlay Image(s).</summary>
        public void SetOverlaySprite(Sprite sprite) =>
            ApplyToOverlayImages(img => ApplySpriteToImage(img, sprite));

        // ── Cell-image mode ───────────────────────────────────────────────────────────

        /// <summary>
        /// Dùng cho <see cref="BlockShapeType.Corner2x2MissingOne"/>: ẩn <c>blockImage</c> gốc
        /// và tạo một <see cref="Image"/> con cho mỗi ô trong <paramref name="cells"/>.
        /// <para>
        /// <paramref name="cells"/>: tọa độ ô đã normalize về bbox-min (bắt đầu từ (0,0)).
        /// <br/>Ví dụ MissingTopLeft → (0,0), (1,0), (1,1).
        /// </para>
        /// </summary>
        public void SetupCellImages(
            IReadOnlyList<Vector2Int> cells,
            Vector2 cellSize,
            Vector2 cellSpacing,
            bool invertY)
        {
            ClearCellImages();

            // Ẩn component render gốc — KHÔNG dùng gameObject.SetActive vì
            // blockImage có thể nằm trên chính root GameObject của block,
            // làm tắt cả RectTransform và drag handler.
            if (blockImage   != null) blockImage.enabled   = false;
            if (overlayImage != null) overlayImage.enabled = false;

            foreach (var cell in cells)
            {
                _cellImages.Add(CreateCellImage($"CellBody_{cell.x}_{cell.y}",
                    cell, cellSize, cellSpacing, invertY));

                // Tạo overlay image theo từng ô nếu prototype có sẵn trong prefab
                if (overlayImage != null)
                    _cellOverlayImages.Add(CreateCellImage($"CellOverlay_{cell.x}_{cell.y}",
                        cell, cellSize, cellSpacing, invertY));
            }
        }

        /// <summary>
        /// Tạo một Image con kích thước đúng một ô tại vị trí <paramref name="cell"/>
        /// trong không gian cục bộ của block (pivot top-left).
        /// </summary>
        private Image CreateCellImage(
            string goName,
            Vector2Int cell,
            Vector2 cellSize,
            Vector2 cellSpacing,
            bool invertY)
        {
            var parent = cellImagesRoot != null ? cellImagesRoot : transform;
            var go     = new GameObject(goName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect         = go.GetComponent<RectTransform>();
            rect.anchorMin   = new Vector2(0f, 1f); // anchor top-left — đồng bộ pivot block cha
            rect.anchorMax   = new Vector2(0f, 1f);
            rect.pivot       = new Vector2(0f, 1f);

            // Offset trong không gian rect của block (pivot top-left):
            //   col tăng → phải; row tăng → xuống (y âm khi invertY=true)
            float ox = cell.x * (cellSize.x + cellSpacing.x);
            float oy = (invertY ? -1f : 1f) * cell.y * (cellSize.y + cellSpacing.y);
            rect.anchoredPosition = new Vector2(ox, oy);
            rect.sizeDelta        = cellSize;

            return go.GetComponent<Image>();
        }

        /// <summary>Xóa tất cả cell image con và khôi phục chế độ single-image.</summary>
        private void ClearCellImages()
        {
            foreach (var img in _cellImages)
                if (img != null) Destroy(img.gameObject);
            _cellImages.Clear();

            foreach (var img in _cellOverlayImages)
                if (img != null) Destroy(img.gameObject);
            _cellOverlayImages.Clear();

            // Hiển thị lại component render gốc
            if (blockImage   != null) blockImage.enabled   = true;
            if (overlayImage != null) overlayImage.enabled = true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Áp <paramref name="action"/> lên body images:
        /// dùng _cellImages nếu đang ở cell-image mode, ngược lại dùng <c>blockImage</c>.
        /// </summary>
        private void ApplyToBodyImages(Action<Image> action)
        {
            if (_cellImages.Count > 0)
                foreach (var img in _cellImages) action(img);
            else if (blockImage != null)
                action(blockImage);
        }

        /// <summary>
        /// Áp <paramref name="action"/> lên overlay images:
        /// dùng _cellOverlayImages nếu đang ở cell-image mode, ngược lại dùng <c>overlayImage</c>.
        /// </summary>
        private void ApplyToOverlayImages(Action<Image> action)
        {
            if (_cellOverlayImages.Count > 0)
                foreach (var img in _cellOverlayImages) action(img);
            else if (overlayImage != null)
                action(overlayImage);
        }

        private static void ApplySpriteToImage(Image img, Sprite sprite)
        {
            img.sprite = sprite;
            img.type   = sprite != null && sprite.border != Vector4.zero
                ? Image.Type.Sliced
                : Image.Type.Simple;
        }

        // ── Arrow API ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tạo 4 mũi tên lần đầu (nếu chưa có) hoặc cập nhật vị trí/sprite/size khi reuse.
        /// Arrows KHÔNG bị destroy — tồn tại cùng block suốt vòng đời pool.
        /// Gọi sau <c>ResizeView</c> (cần <c>sizeDelta</c> đã được set).
        /// </summary>
        /// <param name="sprite">Sprite mũi tên (mặc định trỏ LÊN, 0°).</param>
        /// <param name="color">Màu tint.</param>
        /// <param name="size">Kích thước cạnh mỗi mũi tên (pixels).</param>
        /// <param name="offset">Khoảng lùi từ cạnh block vào tâm mũi tên (pixels).</param>
        public void SetupArrows(Sprite sprite, Color color, float size, float offset)
        {
            var w    = GetComponent<RectTransform>().sizeDelta.x;
            var h    = GetComponent<RectTransform>().sizeDelta.y;
            var half = size * 0.5f;

            // Sprite gốc trỏ LÊN (0°). Góc xoay Z (CCW = dương):
            //   Up = 0°,  Right = -90°,  Down = 180°,  Left = 90°
            // Vị trí BÊN TRONG block (pivot top-left: y=0 ở trên, y=-h ở dưới):
            _arrowLeft  = EnsureArrow(ref _arrowLeft,  "Arrow_Left",  sprite, color, size,
                new Vector2(offset + half,      -h * 0.5f),           90f);
            _arrowRight = EnsureArrow(ref _arrowRight, "Arrow_Right", sprite, color, size,
                new Vector2(w - offset - half,  -h * 0.5f),          -90f);
            _arrowUp    = EnsureArrow(ref _arrowUp,    "Arrow_Up",    sprite, color, size,
                new Vector2(w * 0.5f,           -(offset + half)),      0f);
            _arrowDown  = EnsureArrow(ref _arrowDown,  "Arrow_Down",  sprite, color, size,
                new Vector2(w * 0.5f,           -h + offset + half),  180f);
        }

        /// <summary>
        /// Hiện / ẩn từng mũi tên bằng hiệu ứng zoom in/out (PrimeTween Scale).
        /// Visibility được điều khiển hoàn toàn bằng <c>localScale</c> —
        /// arrows không bao giờ bị disable hay destroy.
        /// </summary>
        public void UpdateArrows(bool showLeft, bool showRight, bool showUp, bool showDown)
        {
            AnimateArrow(_arrowLeft,  showLeft);
            AnimateArrow(_arrowRight, showRight);
            AnimateArrow(_arrowUp,    showUp);
            AnimateArrow(_arrowDown,  showDown);
        }

        /// <summary>
        /// Zoom in (0→1, OutBack 0.2s) hoặc zoom out (1→0, InBack 0.15s).
        /// Tween cũ trên cùng transform bị hủy trước khi tween mới bắt đầu.
        /// </summary>
        private static void AnimateArrow(Image img, bool show)
        {
            if (img == null) return;
            var t = img.transform;

            if (show)
            {
                if (t.localScale == Vector3.one) return; // đã hiển thị đầy đủ, bỏ qua
                Tween.StopAll(t);
                t.localScale = Vector3.zero; // đảm bảo bắt đầu từ 0 nếu tween hide bị ngắt
                Tween.Scale(t, Vector3.one,  0.2f,  Ease.OutBack);
            }
            else
            {
                if (t.localScale == Vector3.zero) return; // đã ẩn hoàn toàn, bỏ qua
                Tween.StopAll(t);
                Tween.Scale(t, Vector3.zero, 0.15f, Ease.InBack);
            }
        }

        /// <summary>
        /// Tạo arrow mới nếu <paramref name="existing"/> là null,
        /// ngược lại chỉ cập nhật position/size/sprite/rotation trên object hiện có.
        /// Scale được reset về 0 (ẩn) trong cả hai trường hợp.
        /// </summary>
        private Image EnsureArrow(ref Image existing, string goName, Sprite sprite,
            Color color, float size, Vector2 localPos, float zRotation)
        {
            RectTransform rect;
            Image img;

            if (existing == null)
            {
                // Tạo lần đầu — spawn dưới ArrowsRoot
                var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(ArrowsRoot, false);
                rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot     = new Vector2(0.5f, 0.5f);
                img = go.GetComponent<Image>();
            }
            else
            {
                // Reuse — chỉ update layout
                img  = existing;
                rect = existing.GetComponent<RectTransform>();
                Tween.StopAll(rect); // hủy tween đang chạy trước khi đặt lại scale
            }

            rect.anchoredPosition = localPos;
            rect.sizeDelta        = new Vector2(size, size);
            rect.localRotation    = Quaternion.Euler(0f, 0f, zRotation);
            rect.localScale       = Vector3.zero; // ẩn mặc định; UpdateArrows sẽ animate

            img.sprite = sprite;
            img.color  = color;
            img.enabled = true; // luôn enabled, scale=0 là trạng thái "ẩn"
            return img;
        }

        /// <summary>
        /// Dừng mọi tween và reset scale về 0 (ẩn tức thì, không animate).
        /// Gọi khi trả block về pool — arrows KHÔNG bị destroy.
        /// </summary>
        private void ResetArrowsToHidden()
        {
            foreach (var img in new[] { _arrowLeft, _arrowRight, _arrowUp, _arrowDown })
            {
                if (img == null) continue;
                Tween.StopAll(img.transform);
                img.transform.localScale = Vector3.zero;
            }
        }

        // ── Pool ─────────────────────────────────────────────────────────────────────

        public void OnGetFromPool() { }

        public void OnReturnToPool()
        {
            ClearCellImages();    // hủy image con, khôi phục blockImage/overlayImage
            ResetArrowsToHidden(); // ẩn tức thì, không destroy
            BlockId    = null;
            _presenter = null;
        }
    }
}


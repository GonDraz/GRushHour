using UnityEngine;

namespace GonDraz.Extensions
{
    public static class CameraExtensions
    {
        /// <summary>
        ///     Converts a world position to a position on the UI Canvas.
        ///     (Chuyển vị trí thế giới sang vị trí trên UI Canvas)
        /// </summary>
        /// <param name="cam">The camera used for the conversion.</param>
        /// <param name="canvas">The canvas to which the position will be converted.</param>
        /// <param name="worldPos">The world position to be converted.</param>
        /// <returns>A Vector2 representing the position on the UI Canvas.</returns>
        public static Vector2 WorldToCanvas(this Camera cam, Canvas canvas, Vector3 worldPos)
        {
            Vector2 viewportPosition = cam.WorldToViewportPoint(worldPos);
            var canvasRect = canvas.GetComponent<RectTransform>();
            return new Vector2(
                viewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
                viewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);
        }

        /// <summary>
        ///     Checks if a renderer is visible from the camera.
        ///     (Kiểm tra renderer có nằm trong vùng nhìn thấy của camera không)
        /// </summary>
        /// <param name="cam">The camera used for the visibility check.</param>
        /// <param name="renderer">The renderer to be checked.</param>
        /// <returns>True if the renderer is visible from the camera, otherwise false.</returns>
        public static bool IsVisibleFrom(this Camera cam, Renderer renderer)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        /// <summary>
        ///     Gets the bounds of the camera's view frustum at the far clip plane.
        ///     (Lấy kích thước vùng nhìn thấy của camera tại far clip plane)
        /// </summary>
        /// <param name="cam">The camera whose view bounds are to be calculated.</param>
        /// <returns>A Bounds structure representing the size of the visible area.</returns>
        public static Bounds GetViewBounds(this Camera cam)
        {
            var frustumCorners = new Vector3[4];
            cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono,
                frustumCorners);
            var worldPos = cam.transform.position;
            var bounds = new Bounds(worldPos, Vector3.zero);
            foreach (var corner in frustumCorners)
                bounds.Encapsulate(cam.transform.TransformPoint(corner));
            return bounds;
        }

        /// <summary>
        ///     Converts a screen position to a world position on the UI Canvas.
        ///     (Chuyển vị trí màn hình sang vị trí thế giới trên UI Canvas)
        /// </summary>
        public static Vector3 ScreenToWorldPointUI(this Camera cam, Canvas canvas, Vector2 screenPos)
        {
            var canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out pos);
            return canvas.transform.TransformPoint(pos);
        }

        /// <summary>
        ///     Converts a world position to a screen position. Returns null if not visible.
        ///     (Chuyển vị trí thế giới sang vị trí màn hình, trả về null nếu ngoài camera)
        /// </summary>
        public static Vector2? WorldToScreenPointSafe(this Camera cam, Vector3 worldPos)
        {
            var screen = cam.WorldToScreenPoint(worldPos);
            if (screen.z < 0) return null;
            if (screen.x < 0 || screen.x > Screen.width || screen.y < 0 || screen.y > Screen.height) return null;
            return new Vector2(screen.x, screen.y);
        }

        /// <summary>
        ///     Gets the world positions of the camera frustum corners at a given distance.
        ///     (Lấy danh sách các góc của frustum camera ở khoảng cách chỉ định)
        /// </summary>
        public static Vector3[] GetFrustumCornersWorld(this Camera cam, float distance)
        {
            var corners = new Vector3[4];
            cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), distance, Camera.MonoOrStereoscopicEye.Mono, corners);
            for (var i = 0; i < 4; i++)
                corners[i] = cam.transform.TransformPoint(corners[i]);
            return corners;
        }

        /// <summary>
        ///     Checks if a world point is visible from the camera.
        ///     (Kiểm tra một điểm có nằm trong vùng nhìn thấy của camera không)
        /// </summary>
        public static bool IsPointVisible(this Camera cam, Vector3 worldPoint)
        {
            var viewport = cam.WorldToViewportPoint(worldPoint);
            return viewport.z > 0 && viewport.x >= 0 && viewport.x <= 1 && viewport.y >= 0 && viewport.y <= 1;
        }

        /// <summary>
        ///     Gets the orthographic bounds of the camera (for 2D games).
        ///     (Lấy bounds vùng nhìn thấy của camera khi ở chế độ orthographic)
        /// </summary>
        public static Bounds GetOrthographicBounds(this Camera cam)
        {
            if (!cam.orthographic) return default;
            var screenAspect = (float)Screen.width / Screen.height;
            var cameraHeight = cam.orthographicSize * 2;
            return new Bounds(
                cam.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, cam.farClipPlane - cam.nearClipPlane)
            );
        }

        /// <summary>
        ///     Gets the view rectangle of the camera in screen coordinates.
        ///     (Lấy Rect vùng nhìn thấy của camera trên màn hình)
        /// </summary>
        public static Rect GetViewRect(this Camera cam)
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
    }
}
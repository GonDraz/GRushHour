using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GonDraz.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        ///     Resets the local position, rotation, and scale of the Transform to default values.
        ///     (Đặt lại vị trí, xoay, scale của Transform về mặc định)
        /// </summary>
        /// <param name="t"></param>
        public static void ResetTransform(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        ///     Sets the X position of the Transform in world space.
        ///     (Đặt vị trí X của Transform trong không gian thế giới)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="x"></param>
        public static void SetPositionX(this Transform t, float x)
        {
            t.position = new Vector3(x, t.position.y, t.position.z);
        }

        /// <summary>
        ///     Sets the Y position of the Transform in world space.
        ///     (Đặt vị trí Y của Transform trong không gian thế giới)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="y"></param>
        public static void SetPositionY(this Transform t, float y)
        {
            t.position = new Vector3(t.position.x, y, t.position.z);
        }

        /// <summary>
        ///     Sets the Z position of the Transform in world space.
        ///     (Đặt vị trí Z của Transform trong không gian thế giới)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="z"></param>
        public static void SetPositionZ(this Transform t, float z)
        {
            t.position = new Vector3(t.position.x, t.position.y, z);
        }

        /// <summary>
        ///     Returns a list of all direct child Transforms.
        ///     (Lấy danh sách các Transform con trực tiếp)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<Transform> GetChildren(this Transform t)
        {
            return t.Cast<Transform>().ToList();
        }

        /// <summary>
        ///     Moves the Transform to the target world position.
        ///     (Di chuyển Transform tới vị trí chỉ định)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="target"></param>
        public static void MoveTo(this Transform t, Vector3 target)
        {
            t.position = target;
        }

        /// <summary>
        ///     Rotates the Transform to the target rotation.
        ///     (Xoay Transform tới góc chỉ định)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="rotation"></param>
        public static void RotateTo(this Transform t, Quaternion rotation)
        {
            t.rotation = rotation;
        }

        /// <summary>
        ///     Returns the full hierarchy path of the Transform.
        ///     (Lấy đường dẫn đầy đủ của Transform trong hierarchy)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetFullPath(this Transform t)
        {
            var path = t.name;
            var current = t.parent;
            while (current)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        /// <summary>
        ///     Sets the local position, rotation, and scale of the Transform to the given values.
        ///     (Đặt localPosition, localRotation, localScale của Transform theo giá trị chỉ định)
        /// </summary>
        public static void SetLocalTransform(this Transform t, Vector3 localPosition, Quaternion localRotation,
            Vector3 localScale)
        {
            t.localPosition = localPosition;
            t.localRotation = localRotation;
            t.localScale = localScale;
        }

        /// <summary>
        ///     Returns true if the Transform has no children.
        ///     (Kiểm tra Transform không có con)
        /// </summary>
        public static bool IsLeaf(this Transform t)
        {
            return t.childCount == 0;
        }

        /// <summary>
        ///     Returns the root Transform of the hierarchy.
        ///     (Lấy Transform gốc của hierarchy)
        /// </summary>
        public static Transform GetRoot(this Transform t)
        {
            while (t.parent)
                t = t.parent;
            return t;
        }

        /// <summary>
        ///     Sets the parent of the Transform and optionally resets local transform.
        ///     (Đặt parent cho Transform, tuỳ chọn reset local transform)
        /// </summary>
        public static void SetParentAndReset(this Transform t, Transform parent, bool resetLocal = true)
        {
            t.SetParent(parent);
            if (!resetLocal) return;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        ///     Traverses all descendants of the Transform (depth-first).
        ///     (Duyệt toàn bộ các Transform con cháu theo chiều sâu)
        /// </summary>
        public static IEnumerable<Transform> Traverse(this Transform t)
        {
            foreach (Transform child in t)
            {
                yield return child;
                foreach (var descendant in child.Traverse())
                    yield return descendant;
            }
        }

        /// <summary>
        ///     Finds the first child Transform by name (non-recursive).
        ///     (Tìm Transform con trực tiếp theo tên, không đệ quy)
        /// </summary>
        public static Transform FindDirectChild(this Transform t, string name)
        {
            return t.Cast<Transform>().FirstOrDefault(child => child.name == name);
        }

        /// <summary>
        ///     Sets the layer for this Transform and all its children recursively.
        ///     (Đặt layer cho Transform và toàn bộ cây con)
        /// </summary>
        public static void SetLayerRecursively(this Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t)
                child.SetLayerRecursively(layer);
        }

        /// <summary>
        ///     Gets the world-space bounds that encapsulate all Renderers under this Transform.
        ///     (Lấy bounds thế giới bao toàn bộ Renderer dưới Transform này)
        /// </summary>
        public static Bounds GetHierarchyWorldBounds(this Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(t.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        public static void SetParentPosition(this Transform mainTR, Vector3 v)
        {
            var list = new List<Transform>();
            var parent = mainTR.parent;
            for (var i = parent.childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i);
                child.SetParent(parent.parent);
                list.Add(child);
            }

            parent.transform.position = v;

            foreach (var child in list) child.SetParent(parent, true);
        }
    }
}
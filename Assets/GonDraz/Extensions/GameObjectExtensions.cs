using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GonDraz.Extensions
{
    public static class GameObjectExtensions
    {
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        ///     Gets the component of type T if it exists, otherwise adds and returns it.
        ///     (Lấy component kiểu T nếu có, nếu không sẽ tự động thêm mới và trả về)
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.TryGetComponent(out T comp) ? comp : go.AddComponent<T>();
        }

        /// <summary>
        ///     Checks if the GameObject has a component of type T.
        ///     (Kiểm tra GameObject có component kiểu T hay không)
        /// </summary>
        public static bool HasComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>();
        }

        /// <summary>
        ///     Destroys all child GameObjects of this GameObject.
        ///     (Xoá toàn bộ các GameObject con của GameObject này)
        /// </summary>
        public static void DestroyChildren(this GameObject go)
        {
            foreach (Transform child in go.transform) Object.Destroy(child.gameObject);
        }

        /// <summary>
        ///     Resets the local position, rotation, and scale of the GameObject to default values (0,0,0 and 1,1,1).
        ///     (Đặt lại vị trí, xoay, scale của GameObject về mặc định (0,0,0 và 1,1,1))
        /// </summary>
        public static void ResetTransform(this GameObject go)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }

        /// <summary>
        ///     Sets the layer for this GameObject and all its children recursively.
        ///     (Đặt layer cho GameObject và toàn bộ cây con của nó)
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) child.gameObject.SetLayerRecursively(layer);
        }

        /// <summary>
        ///     Checks if the GameObject is in the specified LayerMask.
        ///     (Kiểm tra GameObject có nằm trong LayerMask chỉ định không)
        /// </summary>
        public static bool IsInLayerMask(this GameObject go, LayerMask mask)
        {
            return (mask.value & (1 << go.layer)) != 0;
        }

        /// <summary>
        ///     Finds a child Transform by name in the entire hierarchy (including inactive children).
        ///     (Tìm Transform con theo tên trong toàn bộ cây con, kể cả các con ẩn)
        /// </summary>
        public static Transform FindChildByName(this GameObject go, string name)
        {
            return go.transform.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.name == name);
        }

        /// <summary>
        ///     Sets the active state for all direct children of this GameObject.
        ///     (Bật/tắt toàn bộ các GameObject con trực tiếp)
        /// </summary>
        public static void SetActiveChildren(this GameObject go, bool active)
        {
            foreach (Transform child in go.transform) child.gameObject.SetActive(active);
        }

        /// <summary>
        ///     Sets the active state recursively for this GameObject and all its children.
        ///     (Bật/tắt toàn bộ cây con của GameObject này)
        /// </summary>
        [Obsolete("Obsolete")]
        public static void SetActiveRecursively(this GameObject go, bool value)
        {
            go.SetActive(value);
            foreach (Transform child in go.transform) child.gameObject.SetActiveRecursively(value);
        }

        /// <summary>
        ///     Checks if the GameObject is visible from the specified camera.
        ///     (Kiểm tra GameObject có nằm trong vùng nhìn thấy của camera không)
        /// </summary>
        public static bool IsVisibleFrom(this GameObject go, Camera cam)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return false;

            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        /// <summary>
        ///     Checks if the GameObject's tag matches any of the provided tags.
        ///     (So sánh tag của GameObject với nhiều lựa chọn tag)
        /// </summary>
        public static bool CompareTagIn(this GameObject go, params string[] tags)
        {
            return tags.Any(go.CompareTag);
        }

        /// <summary>
        ///     Logs the full hierarchy path of the GameObject to the console.
        ///     (In ra đường dẫn đầy đủ của GameObject trong hierarchy)
        /// </summary>
        public static void LogHierarchyPath(this GameObject go)
        {
            var path = go.name;
            var current = go.transform.parent;
            while (current)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            Debug.Log("Hierarchy Path: " + path);
        }

        /// <summary>
        ///     Rotates the GameObject to look at a target position in 2D space.
        ///     (Xoay GameObject để nhìn về vị trí chỉ định trong không gian 2D)
        /// </summary>
        public static void LookAt2D(this GameObject go, Vector3 targetPosition)
        {
            var dir = targetPosition - go.transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        ///     Gets the normalized direction vector from this GameObject to another GameObject.
        ///     (Lấy vector hướng chuẩn hoá từ GameObject này đến GameObject khác)
        /// </summary>
        public static Vector3 GetDirectionTo(this GameObject go, GameObject target)
        {
            return (target.transform.position - go.transform.position).normalized;
        }

        /// <summary>
        ///     Gets all components of type T in all children (excluding the root GameObject).
        ///     (Lấy tất cả component kiểu T ở các con, không lấy ở chính GameObject gốc)
        /// </summary>
        public static T[] GetComponentsInChildrenOnly<T>(this GameObject go, bool includeInactive = false)
            where T : Component
        {
            var all = go.GetComponentsInChildren<T>(includeInactive);
            var self = go.GetComponent<T>();
            if (!self) return all;
            var list = new List<T>(all);
            list.Remove(self);
            return list.ToArray();
        }

        /// <summary>
        ///     Finds the first child Transform with the specified tag in the hierarchy.
        ///     (Tìm Transform con đầu tiên theo tag chỉ định trong toàn bộ cây con)
        /// </summary>
        public static Transform FindChildByTag(this GameObject go, string tag)
        {
            return go.transform.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.CompareTag(tag));
        }

        /// <summary>
        ///     Finds a component of type T in the parent GameObjects.
        ///     (Tìm component kiểu T ở các GameObject cha)
        /// </summary>
        public static T FindComponentInParent<T>(this GameObject go) where T : Component
        {
            return go.GetComponentInParent<T>();
        }

        /// <summary>
        ///     Sets the tag for this GameObject and all its children recursively.
        ///     (Đặt tag cho toàn bộ cây con)
        /// </summary>
        public static void SetTagRecursively(this GameObject go, string tag)
        {
            go.tag = tag;
            foreach (Transform child in go.transform) child.gameObject.SetTagRecursively(tag);
        }

        /// <summary>
        ///     Instantiates (clones) this GameObject at a given position and parent.
        ///     (Tạo bản sao của GameObject, có thể chỉ định vị trí và cha mới)
        /// </summary>
        public static GameObject Clone(this GameObject go, Vector3? position = null, Transform parent = null)
        {
            return Object.Instantiate(go, position ?? go.transform.position, go.transform.rotation, parent);
        }

        /// <summary>
        ///     Enables or disables all components of type T on this GameObject and its children.
        ///     (Bật/tắt toàn bộ các component kiểu T trên GameObject và cây con)
        /// </summary>
        public static void SetEnabledAll<T>(this GameObject go, bool enabled) where T : Behaviour
        {
            foreach (var comp in go.GetComponentsInChildren<T>(true)) comp.enabled = enabled;
        }

        /// <summary>
        ///     Gets the full hierarchy path of this GameObject as a string.
        ///     (Lấy đường dẫn hierarchy dạng string, ví dụ: Root/Child/SubChild)
        /// </summary>
        public static string GetPathInHierarchy(this GameObject go)
        {
            var path = go.name;
            var current = go.transform.parent;
            while (current)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        /// <summary>
        ///     Removes the component of type T from this GameObject if it exists.
        ///     (Xoá component kiểu T nếu có)
        /// </summary>
        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp) Object.Destroy(comp);
        }

        /// <summary>
        ///     Gets all child GameObjects recursively (all descendants).
        ///     (Lấy danh sách tất cả GameObject con, bao gồm các cấp con cháu)
        /// </summary>
        public static List<GameObject> GetAllChildrenRecursive(this GameObject go)
        {
            var list = new List<GameObject>();
            foreach (Transform child in go.transform)
            {
                list.Add(child.gameObject);
                list.AddRange(child.gameObject.GetAllChildrenRecursive());
            }

            return list;
        }

        public static T AddComponentIfNotExists<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component)) component = gameObject.AddComponent<T>();

            return component;
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Checks if this GameObject is a prefab asset (Editor only).
        ///     (Kiểm tra GameObject có phải là prefab không - chỉ dùng trong Editor)
        /// </summary>
        public static bool IsPrefab(this GameObject go)
        {
            return PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab;
        }
#endif
    }
}
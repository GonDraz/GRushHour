using System;
using System.Collections.Generic;
using UnityEngine;

namespace GonDraz.UI.Route
{
    /// <summary>
    ///     Quản lý navigation và hiển thị các UI presentations sử dụng pattern stack-based.
    ///     Cho phép điều hướng giữa các màn hình với khả năng quay lại màn hình trước đó.
    /// </summary>
    public static partial class RouteManager
    {
        /// <summary>
        ///     Lưu trữ mapping giữa Type của presentation và instance của nó.
        /// </summary>
        private static readonly Dictionary<Type, Presentation> Presentations = new();

        /// <summary>
        ///     Stack quản lý lịch sử điều hướng. Mỗi phần tử là một mảng các Type presentations.
        /// </summary>
        private static readonly Stack<Type[]> StackBased = new();

        /// <summary>
        ///     Transform cha chứa tất cả các UI presentations.
        /// </summary>
        public static Transform routeParent;

        /// <summary>
        ///     Thiết lập transform cha cho tất cả các presentations.
        /// </summary>
        /// <param name="parent">Transform cha mà các presentations sẽ được thêm vào</param>
        public static void SetRouteParent(Transform parent)
        {
            routeParent = parent;
        }

        /// <summary>
        ///     Điều hướng đến màn hình mới, xóa hết stack cũ (equivalent với "Go home").
        ///     Ẩn tất cả presentations hiện tại và hiển thị những cái mới.
        /// </summary>
        /// <param name="types">Các Type của presentations cần hiển thị</param>
        public static void Go(params Type[] types)
        {
            ClearPresentations();
            ShowPresentations(types);
            StackBased.Push(types);
        }

        /// <summary>
        ///     Thay thế màn hình hiện tại bằng một màn hình mới, nhưng giữ lại khả năng quay lại.
        ///     Ẩn màn hình hiện tại, hiển thị màn hình mới, rồi push cả hai vào stack.
        /// </summary>
        /// <param name="types">Các Type của presentations cần hiển thị</param>
        public static void PushReplacement(params Type[] types)
        {
            var oldType = StackBased.Peek();
            ClearPresentations();
            ShowPresentations(types);
            StackBased.Push(oldType);
            StackBased.Push(types);
        }

        /// <summary>
        ///     Thêm một màn hình mới vào stack mà không ẩn màn hình hiện tại.
        ///     Có thể dùng để hiển thị popup/dialog trên top của màn hình hiện tại.
        /// </summary>
        /// <param name="types">Các Type của presentations cần hiển thị</param>
        public static void Push(params Type[] types)
        {
            ShowPresentations(types);
            StackBased.Push(types);
        }

        /// <summary>
        ///     Hiển thị các presentations theo Type, đưa chúng lên trên cùng (SetSiblingIndex).
        /// </summary>
        /// <param name="types">Mảng các Type của presentations cần hiển thị</param>
        private static void ShowPresentations(Type[] types)
        {
            foreach (var type in types)
                if (Presentations.TryGetValue(type, out var widget))
                {
                    if (widget.gameObject.activeInHierarchy) continue;
                    widget.Show();
                    // Đặt presentation này ở cuối danh sách (top most layer)
                    widget.transform.SetSiblingIndex(routeParent.childCount);
                }
        }

        /// <summary>
        ///     Ẩn các presentations theo Type, đưa chúng xuống dưới cùng.
        /// </summary>
        /// <param name="types">Mảng các Type của presentations cần ẩn</param>
        private static void HidePresentations(Type[] types)
        {
            foreach (var type in types)
                if (Presentations.TryGetValue(type, out var widget))
                    widget.Hide();
            // Đặt presentation này ở đầu danh sách (bottom most layer)
            // widget.transform.SetSiblingIndex(0);
        }

        /// <summary>
        ///     Ẩn tất cả presentations và xóa stack lịch sử điều hướng.
        ///     Dùng khi muốn reset toàn bộ trạng thái.
        /// </summary>
        private static void ClearPresentations()
        {
            foreach (var presentation in Presentations.Values) presentation.Hide();
            StackBased.Clear();
        }

        /// <summary>
        ///     Quay lại màn hình trước đó trong stack.
        ///     Nếu chỉ còn 1 màn hình, thoát ứng dụng (ngoài chế độ Editor).
        /// </summary>
        public static void Pop()
        {
            if (StackBased.Count > 1)
            {
                // Pop màn hình hiện tại
                if (!StackBased.TryPop(out var types)) return;
                HidePresentations(types);
                // Hiển thị màn hình trước đó
                ShowPresentations(StackBased.Peek());
            }
            //Application.Quit();
        }

        public static T GetPresentation<T>() where T : Presentation
        {
            var type = typeof(T);
            if (Presentations.TryGetValue(type, out var presentation))
                return presentation as T;
            Debug.LogError($"Presentation of type {type} not found!");
            return null;
        }
    }
}
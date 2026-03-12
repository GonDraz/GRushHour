using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace GonDraz.Extensions
{
    public static class UIExtensions
    {
        /// <summary>
        ///     Sets the alpha value for a UI Graphic component.
        ///     (Đặt giá trị alpha cho thành phần UI Graphic)
        /// </summary>
        /// <param name="ui">The Graphic component of the UI.</param>
        /// <param name="alpha">The alpha value to be set.</param>
        public static void SetAlpha(this Graphic ui, float alpha)
        {
            var color = ui.color;
            color.a = alpha;
            ui.color = color;
        }

        /// <summary>
        ///     Safely sets the text for a UnityEngine.UI.Text component.
        ///     (Đặt text an toàn cho Text, không bị null)
        /// </summary>
        /// <param name="ui">The Text component of the UI.</param>
        /// <param name="text">The text value to be set.</param>
        public static void SetTextSafe(this Text ui, string text)
        {
            if (ui) ui.text = text ?? string.Empty;
        }

        /// <summary>
        ///     Safely sets the text for a TextMeshProUGUI component.
        ///     (Đặt text an toàn cho TextMeshProUGUI, không bị null)
        /// </summary>
        /// <param name="ui">The TextMeshProUGUI component of the UI.</param>
        /// <param name="text">The text value to be set.</param>
        public static void SetTextSafe(this TextMeshProUGUI ui, string text)
        {
            if (ui) ui.text = text ?? string.Empty;
        }

        /// <summary>
        ///     Sets the color for a UI Graphic component.
        ///     (Đổi màu cho thành phần UI Graphic)
        /// </summary>
        /// <param name="ui">The Graphic component of the UI.</param>
        /// <param name="color">The color value to be set.</param>
        public static void SetColor(this Graphic ui, Color color)
        {
            ui.color = color;
        }

        /// <summary>
        ///     Sets the interactable state for a UI Selectable component.
        ///     (Bật/tắt khả năng tương tác cho Selectable)
        /// </summary>
        /// <param name="ui">The Selectable component of the UI.</param>
        /// <param name="interactable">The interactable state to be set.</param>
        public static void SetInteractable(this Selectable ui, bool interactable)
        {
            if (ui) ui.interactable = interactable;
        }

        /// <summary>
        ///     Sets the alpha value for all child Graphics of a GameObject (recursive).
        ///     (Đặt alpha cho tất cả Graphic con của GameObject, đệ quy)
        /// </summary>
        public static void SetAlphaRecursively(this GameObject go, float alpha)
        {
            foreach (var graphic in go.GetComponentsInChildren<Graphic>(true))
            {
                var color = graphic.color;
                color.a = alpha;
                graphic.color = color;
            }
        }

        /// <summary>
        ///     Enables or disables a CanvasGroup (and optionally all child CanvasGroups).
        ///     (Bật/tắt CanvasGroup, tuỳ chọn áp dụng cho toàn bộ cây con)
        /// </summary>
        public static void SetCanvasGroupInteractable(this GameObject go, bool interactable, bool recursive = false)
        {
            if (recursive)
            {
                foreach (var cg in go.GetComponentsInChildren<CanvasGroup>(true))
                    cg.interactable = interactable;
            }
            else
            {
                var cg = go.GetComponent<CanvasGroup>();
                if (cg) cg.interactable = interactable;
            }
        }

        /// <summary>
        ///     Sets the raycast target property for all child Graphics of a GameObject (recursive).
        ///     (Bật/tắt khả năng nhận raycast cho tất cả Graphic con)
        /// </summary>
        public static void SetRaycastTargetRecursively(this GameObject go, bool value)
        {
            foreach (var graphic in go.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = value;
        }

        /// <summary>
        ///     Sets the text color for a UnityEngine.UI.Text or TextMeshProUGUI component.
        ///     (Đặt màu cho Text hoặc TextMeshProUGUI)
        /// </summary>
        public static void SetTextColor(this GameObject go, Color color)
        {
            var uiText = go.GetComponent<Text>();
            if (uiText) uiText.color = color;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp) tmp.color = color;
        }

        /// <summary>
        ///     Sets the font size for a UnityEngine.UI.Text or TextMeshProUGUI component.
        ///     (Đặt kích thước font cho Text hoặc TextMeshProUGUI)
        /// </summary>
        public static void SetFontSize(this GameObject go, float size)
        {
            var uiText = go.GetComponent<Text>();
            if (uiText) uiText.fontSize = (int)size;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp) tmp.fontSize = size;
        }

        /// <summary>
        ///     Sets the sprite for a UnityEngine.UI.Image component.
        ///     (Đặt sprite cho Image)
        /// </summary>
        public static void SetSprite(this GameObject go, Sprite sprite)
        {
            var img = go.GetComponent<Image>();
            if (img) img.sprite = sprite;
        }

        /// <summary>
        ///     Sets the fill amount for a UnityEngine.UI.Image component.
        ///     (Đặt fillAmount cho Image)
        /// </summary>
        public static void SetFillAmount(this GameObject go, float fillAmount)
        {
            var img = go.GetComponent<Image>();
            if (img) img.fillAmount = fillAmount;
        }

        /// <summary>
        ///     Sets the interactable state for all child Selectables of a GameObject (recursive).
        ///     (Bật/tắt khả năng tương tác cho tất cả Selectable con)
        /// </summary>
        public static void SetInteractableRecursively(this GameObject go, bool interactable)
        {
            foreach (var selectable in go.GetComponentsInChildren<Selectable>(true))
                selectable.interactable = interactable;
        }

        public static async UniTaskVoid UpdateLayout(this RectTransform rectTransform)
        {
            await UniTask.Yield();
            Canvas.ForceUpdateCanvases();

            var rectTransforms = new List<RectTransform> { rectTransform };
            rectTransforms.AddRange(rectTransform.GetComponentsInChildren<RectTransform>());

            for (var i = rectTransforms.Count - 1; i >= 0; i--)
            {
                var rt = rectTransforms[i];
                if (rt.GetComponent<ILayoutElement>() != null || rt.GetComponent<ILayoutGroup>() != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        public static Vector2 GetUIPoint(this Transform transform, RectTransform parentRect)
        {
            return GetUIPoint(transform.position, parentRect);
        }

        public static Vector2 GetUIPoint(Vector2 position, RectTransform parentRect)
        {
            var anchoredPos = Vector2.zero;
            if (parentRect)
            {
                var screenPoint = Camera.main
                    ? Camera.main.WorldToScreenPoint(position)
                    : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null,
                    out anchoredPos);
            }

            return anchoredPos;
        }

        /// <summary>
        ///     Convert từ UI RectTransform → World Position
        ///     Dùng cho Canvas Screen Space - Overlay
        /// </summary>
        public static Vector3 GetWorldPointFromUI(this RectTransform rectTransform, Camera worldCamera,
            float worldZ = 0)
        {
            if (!rectTransform || !worldCamera)
                return Vector3.zero;

            // UI → Screen
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

            // Screen → World
            var worldPoint = worldCamera.ScreenToWorldPoint(
                new Vector3(screenPoint.x, screenPoint.y, worldZ)
            );

            return worldPoint;
        }
    }
}
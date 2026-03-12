#if UNITY_EDITOR

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GonDraz.Editor
{
    public class OptimizedUICreatorWindow : EditorWindow
    {
        private bool defaultRaycastTarget = true;
        private bool defaultRichText = true;
        private bool defaultTransition = true;
        private Color defaultUIColor = Color.white;

        private void OnGUI()
        {
            GUILayout.Label("⚙ UI Optimization Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            defaultRaycastTarget = EditorGUILayout.Toggle("Disable Raycast Target", defaultRaycastTarget);
            defaultRichText = EditorGUILayout.Toggle("Disable Rich Text (Text)", defaultRichText);
            defaultTransition = EditorGUILayout.Toggle("No UI Transition", defaultTransition);
            defaultUIColor = EditorGUILayout.ColorField("UI Default Color", defaultUIColor);

            EditorGUILayout.Space();
            GUILayout.Label("📦 UI Elements", EditorStyles.boldLabel);

            if (GUILayout.Button("Image")) CreateImage();
            if (GUILayout.Button("Text")) CreateText();
            if (GUILayout.Button("Button")) CreateButton();
            if (GUILayout.Button("Input Field")) CreateInputField();
            if (GUILayout.Button("Scroll View")) CreateScrollView();
            if (GUILayout.Button("Toggle")) CreateToggle();
            if (GUILayout.Button("Slider")) CreateSlider();
            if (GUILayout.Button("Dropdown")) CreateDropdown();

            EditorGUILayout.Space();
            GUILayout.Label("Optimization UI", EditorStyles.boldLabel);
            if (GUILayout.Button("Disable Raycast in Children")) DisableRaycastInChildren();
            if (GUILayout.Button("Disable Maskable in Children")) DisableMaskableInChildren();
            if (GUILayout.Button("Using empty UIText")) UsingEmptyUIText();
        }

        [MenuItem("GonDraz/Optimized UI Creator")]
        public static void ShowWindow()
        {
            GetWindow<OptimizedUICreatorWindow>("Optimized UI Creator");
        }

        private GameObject CreateTMPText(string name, string content, Transform parent,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center, Color? color = null, bool raycast = false,
            bool? richText = null, float fontSize = 24)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.alignment = alignment;
            tmp.color = color ?? Color.black;
            tmp.raycastTarget = raycast;
            tmp.richText = richText ?? !defaultRichText;
            tmp.fontSize = fontSize;

            var rect = tmp.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return go;
        }

        private static void SelectElement(GameObject go)
        {
            Selection.activeGameObject = go;
            EditorApplication.delayCall += () => EditorGUIUtility.PingObject(go);
        }

        private static void SetParent(GameObject obj)
        {
            if (!Selection.activeTransform || !Selection.activeTransform.GetComponent<RectTransform>())
            {
                DestroyImmediate(obj);
                return;
            }

            obj.transform.SetParent(Selection.activeTransform.parent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            SelectElement(obj);
        }

        private static void CheckEventSystem()
        {
            if (!FindFirstObjectByType<EventSystem>())
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void DisableRaycastInChildren()
        {
            var gameObjects = Selection.gameObjects;
            foreach (var gameObject in gameObjects)
            {
                var graphics = gameObject.GetComponentsInChildren<Graphic>(true);
                foreach (var graphic in graphics)
                {
                    if (!graphic.GetComponent<Selectable>()) graphic.raycastTarget = false;

                    Debug.Log($"Disabled Raycast Target for {graphic.name} in {gameObject.name}");
                }
            }
        }

        private void DisableMaskableInChildren()
        {
            var gameObjects = Selection.gameObjects;
            foreach (var gameObject in gameObjects)
            {
                var images = gameObject.GetComponentsInChildren<Image>(true);
                foreach (var image in images)
                {
                    image.maskable = false;
                    Debug.Log($"Disabled maskable Target for {image.name} in {gameObject.name}");
                }

                var texts = gameObject.GetComponentsInChildren<Text>(true);
                foreach (var text in texts)
                {
                    text.maskable = false;
                    Debug.Log($"Disabled maskable Target for {text.name} in {gameObject.name}");
                }

                var textPros = gameObject.GetComponentsInChildren<TMP_Text>(true);
                foreach (var text in textPros)
                {
                    text.maskable = false;
                    Debug.Log($"Disabled maskable Target for {text.name} in {gameObject.name}");
                }
            }
        }

        private void UsingEmptyUIText()
        {
            var gameObjects = Selection.gameObjects;
            foreach (var gameObject in gameObjects)
            {
                var graphic = gameObject.GetComponent<Graphic>();
                if (graphic) DestroyImmediate(graphic);

                var text = gameObject.AddComponent<Text>();
                text.font = null;
                text.text = "";
                text.supportRichText = false;
                text.raycastTarget = true;
            }
        }

        #region UI Creators

        private void CreateImage()
        {
            var go = new GameObject("Image", typeof(Image));
            var image = go.GetComponent<Image>();
            image.raycastTarget = !defaultRaycastTarget;
            image.color = defaultUIColor;
            SetParent(go);
        }

        private void CreateText()
        {
            var go = CreateTMPText("Text", "New TMP Text", null);
            SetParent(go);
        }

        private void CreateButton()
        {
            var go = new GameObject("Button", typeof(Image), typeof(Button));
            go.GetComponent<Image>().color = defaultUIColor;

            var btn = go.GetComponent<Button>();
            if (defaultTransition) btn.transition = Selectable.Transition.None;

            CreateTMPText("Text", "Button", go.transform);

            SetParent(go);
        }

        private void CreateInputField()
        {
            var go = new GameObject("InputField", typeof(Image), typeof(TMP_InputField));
            go.GetComponent<Image>().color = defaultUIColor;

            var input = go.GetComponent<TMP_InputField>();
            if (defaultTransition) input.transition = Selectable.Transition.None;

            var placeholder = CreateTMPText("Placeholder", "Enter text...", go.transform,
                color: new Color(0.5f, 0.5f, 0.5f, 0.5f));
            var text = CreateTMPText("Text", "", go.transform);

            input.placeholder = placeholder.GetComponent<TMP_Text>();
            input.textComponent = text.GetComponent<TMP_Text>();

            SetParent(go);
        }

        private void CreateScrollView()
        {
            var go = new GameObject("Scroll View", typeof(Image), typeof(ScrollRect));
            go.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);

            var scroll = go.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport", typeof(RectMask2D), typeof(Image));
            viewport.transform.SetParent(go.transform, false);
            viewport.GetComponent<Image>().color = Color.clear;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);

            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = content.GetComponent<RectTransform>();

            SetParent(go);
        }

        private void CreateToggle()
        {
            var go = new GameObject("Toggle", typeof(Image), typeof(Toggle));
            go.GetComponent<Image>().color = defaultUIColor;

            var toggle = go.GetComponent<Toggle>();

            var checkmark = new GameObject("Checkmark", typeof(Image));
            checkmark.transform.SetParent(go.transform, false);
            checkmark.GetComponent<Image>().color = Color.black;

            var label = CreateTMPText("Label", "Toggle", go.transform);

            toggle.graphic = checkmark.GetComponent<Image>();

            SetParent(go);
        }

        private void CreateSlider()
        {
            var go = new GameObject("Slider", typeof(Image), typeof(Slider));
            go.GetComponent<Image>().color = defaultUIColor;

            var slider = go.GetComponent<Slider>();
            if (defaultTransition) slider.transition = Selectable.Transition.None;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);

            var fill = new GameObject("Fill", typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<Image>().color = Color.green;

            var handle = new GameObject("Handle", typeof(Image));
            handle.transform.SetParent(go.transform, false);
            handle.GetComponent<Image>().color = Color.white;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();

            SetParent(go);
        }

        private void CreateDropdown()
        {
            var go = new GameObject("Dropdown", typeof(Image), typeof(TMP_Dropdown));
            go.GetComponent<Image>().color = defaultUIColor;

            var dropdown = go.GetComponent<TMP_Dropdown>();
            if (defaultTransition) dropdown.transition = Selectable.Transition.None;

            var labelObj = CreateTMPText("Label", "Option A", go.transform);
            dropdown.captionText = labelObj.GetComponent<TMP_Text>();

            var templateObj = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            templateObj.transform.SetParent(go.transform, false);
            templateObj.SetActive(false);

            var viewport = new GameObject("Viewport", typeof(RectMask2D), typeof(Image));
            viewport.transform.SetParent(templateObj.transform, false);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);

            dropdown.template = templateObj.GetComponent<RectTransform>();

            SetParent(go);
        }

        #endregion
    }
}

#endif
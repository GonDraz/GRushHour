#if UNITY_EDITOR
using System.Collections.Generic;
using Puzzle.Data;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Puzzle.Editor
{
#if ODIN_INSPECTOR
    [CustomEditor(typeof(PuzzleLevelSO))]
    public class PuzzleLevelSOEditor : OdinEditor
    {
        private readonly List<string> _errors = new();
        private bool _validated;

        public override void OnInspectorGUI()
        {
            DrawButtons();
            DrawValidationBox();
            EditorGUILayout.Space(4);
            base.OnInspectorGUI();
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("▶  Open Level Editor", GUILayout.Height(28)))
                    PuzzleLevelEditorWindow.Open((PuzzleLevelSO)target);

                if (GUILayout.Button("✔  Validate", GUILayout.Height(28)))
                {
                    PuzzleLevelValidationUtility.Validate((PuzzleLevelSO)target, _errors);
                    _validated = true;
                }
            }

            EditorGUILayout.Space(2);
        }

        private void DrawValidationBox()
        {
            if (!_validated) return;

            if (_errors.Count == 0)
            {
                EditorGUILayout.HelpBox("✔  Level is valid.", MessageType.Info);
            }
            else
            {
                foreach (var err in _errors)
                    EditorGUILayout.HelpBox(err, MessageType.Error);
            }
        }
    }
#else
    [CustomEditor(typeof(PuzzleLevelSO))]
    public class PuzzleLevelSOEditor : UnityEditor.Editor
    {
        private readonly List<string> _errors = new();
        private bool _validated;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("▶  Open Level Editor", GUILayout.Height(28)))
                    PuzzleLevelEditorWindow.Open((PuzzleLevelSO)target);

                if (GUILayout.Button("✔  Validate", GUILayout.Height(28)))
                {
                    PuzzleLevelValidationUtility.Validate((PuzzleLevelSO)target, _errors);
                    _validated = true;
                }
            }

            if (_validated)
            {
                EditorGUILayout.Space(2);
                if (_errors.Count == 0)
                    EditorGUILayout.HelpBox("✔  Level is valid.", MessageType.Info);
                else
                    foreach (var err in _errors)
                        EditorGUILayout.HelpBox(err, MessageType.Error);
            }

            EditorGUILayout.Space(4);
            DrawDefaultInspector();
        }
    }
#endif
}
#endif


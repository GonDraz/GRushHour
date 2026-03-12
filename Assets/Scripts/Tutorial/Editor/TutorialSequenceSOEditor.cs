#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Tutorial.Editor
{
    [CustomEditor(typeof(TutorialSequenceSO))]
    public class TutorialSequenceSOEditor : UnityEditor.Editor
    {
        private SerializedProperty displayNameProp;
        private SerializedProperty stepsProp;
        private SerializedProperty tutorialTypeProp;

        private void OnEnable()
        {
            tutorialTypeProp = serializedObject.FindProperty("tutorialType");
            displayNameProp = serializedObject.FindProperty("displayName");
            stepsProp = serializedObject.FindProperty("steps");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Sequence Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Tutorial Type
            EditorGUILayout.PropertyField(tutorialTypeProp, new GUIContent("Tutorial Type", "Loại tutorial"));
            EditorGUILayout.PropertyField(displayNameProp, new GUIContent("Display Name", "Tên hiển thị"));

            EditorGUILayout.Space();

            // Steps
            EditorGUILayout.LabelField("Tutorial Steps", EditorStyles.boldLabel);

            if (stepsProp.arraySize == 0)
                EditorGUILayout.HelpBox("⚠️ Chưa có bước nào! Thêm Tutorial Step SO vào danh sách.",
                    MessageType.Warning);
            else
                EditorGUILayout.HelpBox($"✓ Tutorial có {stepsProp.arraySize} bước", MessageType.Info);

            EditorGUILayout.PropertyField(stepsProp, new GUIContent("Steps", "Danh sách các bước theo thứ tự"), true);

            EditorGUILayout.Space();

            // Sequence Preview
            if (stepsProp.arraySize > 0)
            {
                EditorGUILayout.LabelField("Sequence Preview", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                for (var i = 0; i < stepsProp.arraySize; i++)
                {
                    var stepProp = stepsProp.GetArrayElementAtIndex(i);
                    if (stepProp.objectReferenceValue != null)
                    {
                        var step = stepProp.objectReferenceValue as TutorialStepSO;
                        if (step != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"Step {i + 1}:", GUILayout.Width(50));
                            EditorGUILayout.LabelField($"Target: {step.targetId}", GUILayout.Width(150));
                            EditorGUILayout.LabelField($"'{step.message}'", EditorStyles.wordWrappedLabel);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Step {i + 1}:", GUILayout.Width(50));
                        EditorGUILayout.LabelField("(Empty)", EditorStyles.helpBox);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Save Location Info
            EditorGUILayout.HelpBox(
                "💡 Lưu file này vào 'Assets/Resources/TutorialSequences/' để TutorialManager có thể load tự động.",
                MessageType.Info);

            // Validation Button
            EditorGUILayout.Space();
            if (GUILayout.Button("Validate Sequence", GUILayout.Height(30))) ValidateSequence();

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateSequence()
        {
            var sequence = target as TutorialSequenceSO;
            if (sequence == null) return;

            var hasErrors = false;
            var report = "=== Validation Report ===\n\n";

            // Check steps
            if (sequence.steps == null || sequence.steps.Count == 0)
            {
                report += "❌ ERROR: No steps defined!\n";
                hasErrors = true;
            }
            else
            {
                report += $"✓ Found {sequence.steps.Count} step(s)\n\n";

                for (var i = 0; i < sequence.steps.Count; i++)
                {
                    var step = sequence.steps[i];
                    if (step == null)
                    {
                        report += $"❌ Step {i + 1}: NULL reference!\n";
                        hasErrors = true;
                    }
                    else
                    {
                        report += $"Step {i + 1}: {step.name}\n";

                        if (string.IsNullOrEmpty(step.targetId))
                        {
                            report += "  ❌ ERROR: Target ID is empty!\n";
                            hasErrors = true;
                        }
                        else
                        {
                            report += $"  ✓ Target: {step.targetId}\n";
                        }

                        if (string.IsNullOrEmpty(step.message))
                            report += "  ⚠️ WARNING: Message is empty\n";
                        else
                            report += $"  ✓ Message: '{step.message}'\n";

                        report += "\n";
                    }
                }
            }

            // Check location
            var assetPath = AssetDatabase.GetAssetPath(sequence);
            if (!assetPath.Contains("Resources/TutorialSequences"))
            {
                report += "\n⚠️ WARNING: This sequence is NOT in Resources/TutorialSequences/ folder!\n";
                report += "It will not be loaded by TutorialManager.\n";
            }
            else
            {
                report += "\n✓ Sequence is in correct Resources folder\n";
            }

            if (hasErrors)
            {
                Debug.LogError(report, sequence);
                EditorUtility.DisplayDialog("Validation Failed",
                    "Có lỗi trong sequence! Check Console để xem chi tiết.", "OK");
            }
            else
            {
                Debug.Log(report, sequence);
                EditorUtility.DisplayDialog("Validation Passed", "Tutorial sequence hợp lệ! ✓", "OK");
            }
        }
    }
}
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tutorial.Editor
{
    [CustomEditor(typeof(TutorialRegistry))]
    public class TutorialRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty registryNameProp;
        private SerializedProperty sequencesProp;

        private void OnEnable()
        {
            registryNameProp = serializedObject.FindProperty("registryName");
            sequencesProp = serializedObject.FindProperty("sequences");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Registry", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Sequences List
            EditorGUILayout.LabelField("Registered Tutorial Sequences", EditorStyles.boldLabel);

            if (sequencesProp.arraySize == 0)
                EditorGUILayout.HelpBox("⚠️ Chưa có tutorial sequence nào được đăng ký!\n" +
                                        "Kéo thả Tutorial Sequence SO vào danh sách bên dưới.", MessageType.Warning);
            else
                EditorGUILayout.HelpBox($"✓ Đã đăng ký {sequencesProp.arraySize} tutorial sequence(s)",
                    MessageType.Info);

            EditorGUILayout.PropertyField(sequencesProp, new GUIContent("Sequences"), true);

            EditorGUILayout.Space();

            // Preview registered tutorials
            if (sequencesProp.arraySize > 0)
            {
                EditorGUILayout.LabelField("Registry Preview", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                for (var i = 0; i < sequencesProp.arraySize; i++)
                {
                    var seqProp = sequencesProp.GetArrayElementAtIndex(i);
                    if (seqProp.objectReferenceValue != null)
                    {
                        var sequence = seqProp.objectReferenceValue as TutorialSequenceSO;
                        if (sequence != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));
                            EditorGUILayout.LabelField($"{sequence.tutorialType}", EditorStyles.boldLabel,
                                GUILayout.Width(120));
                            EditorGUILayout.LabelField($"'{sequence.displayName}'", GUILayout.Width(200));
                            EditorGUILayout.LabelField($"({sequence.StepCount} steps)", EditorStyles.miniLabel);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));
                        EditorGUILayout.LabelField("(Empty Slot)", EditorStyles.helpBox);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Validation Button
            if (GUILayout.Button("Validate Registry", GUILayout.Height(30))) ValidateRegistry();

            EditorGUILayout.Space();

            // Usage Info
            EditorGUILayout.HelpBox(
                "💡 TutorialManager sẽ tự động tìm registry này trong scene khi game start.\n" +
                "Component này nên được đặt trên một GameObject persist (DontDestroyOnLoad).",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateRegistry()
        {
            var registry = target as TutorialRegistry;
            if (registry == null) return;

            registry.Validate();

            var sequences = registry.GetAllSequences();
            var hasErrors = false;
            var report = "=== Registry Validation Report ===\n\n";

            if (sequences.Count == 0)
            {
                report += "❌ ERROR: No sequences registered!\n";
                hasErrors = true;
            }
            else
            {
                report += $"✓ Found {sequences.Count} sequence(s)\n\n";

                var typesSeen = new HashSet<TutorialType>();

                foreach (var sequence in sequences)
                {
                    if (sequence == null)
                    {
                        report += "❌ ERROR: Null sequence in list!\n";
                        hasErrors = true;
                        continue;
                    }

                    report += $"Sequence: {sequence.name}\n";
                    report += $"  Type: {sequence.tutorialType}\n";
                    report += $"  Display Name: {sequence.displayName}\n";
                    report += $"  Steps: {sequence.StepCount}\n";

                    if (typesSeen.Contains(sequence.tutorialType))
                        report += $"  ⚠️ WARNING: Duplicate type {sequence.tutorialType}!\n";
                    else
                        typesSeen.Add(sequence.tutorialType);

                    if (!sequence.HasSteps)
                    {
                        report += "  ❌ ERROR: No steps defined!\n";
                        hasErrors = true;
                    }

                    report += "\n";
                }
            }

            // Check file location
            if (registry.gameObject.scene.name == null || registry.gameObject.scene.name == "")
            {
                report += "\n⚠️ WARNING: Registry is on a PREFAB, not in scene!\n";
                report += "Make sure to place this GameObject in your main/startup scene.\n";
            }
            else
            {
                report += $"\n✓ Registry is in scene: {registry.gameObject.scene.name}\n";
            }

            // Check DontDestroyOnLoad
            if (registry.gameObject.scene.name == "DontDestroyOnLoad")
                report += "✓ Registry is marked as DontDestroyOnLoad\n";
            else
                report += "💡 TIP: Registry will be marked DontDestroyOnLoad on Awake()\n";

            if (hasErrors)
            {
                Debug.LogError(report, registry);
                EditorUtility.DisplayDialog("Validation Failed",
                    "Có lỗi trong registry! Check Console để xem chi tiết.", "OK");
            }
            else
            {
                Debug.Log(report, registry);
                EditorUtility.DisplayDialog("Validation Passed", "Tutorial Registry hợp lệ! ✓", "OK");
            }
        }
    }
}
#endif
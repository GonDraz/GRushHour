#if UNITY_EDITOR && !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace Tutorial.Editor
{
    [CustomEditor(typeof(TutorialStepSO))]
    public class TutorialStepSOEditor : UnityEditor.Editor
    {
        private SerializedProperty targetIdProp;
        private SerializedProperty messageProp;
        private SerializedProperty focusPaddingProp;
        private SerializedProperty blockInputProp;
        private SerializedProperty showHandProp;
        private SerializedProperty handOffsetProp;

        private void OnEnable()
        {
            targetIdProp = serializedObject.FindProperty("targetId");
            messageProp = serializedObject.FindProperty("message");
            focusPaddingProp = serializedObject.FindProperty("focusPadding");
            blockInputProp = serializedObject.FindProperty("blockInput");
            showHandProp = serializedObject.FindProperty("showHand");
            handOffsetProp = serializedObject.FindProperty("handOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // If any property is missing (script changed), fallback to default inspector.
            if (targetIdProp == null || messageProp == null || focusPaddingProp == null ||
                blockInputProp == null || showHandProp == null || handOffsetProp == null)
            {
                EditorGUILayout.HelpBox("TutorialStepSO fields have changed. Showing default inspector.", MessageType.Info);
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Step Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Target Configuration
            EditorGUILayout.LabelField("Target Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(targetIdProp, new GUIContent("Target ID", "ID của UI element (vd: 'play_button')"));
            
            if (string.IsNullOrEmpty(targetIdProp.stringValue))
            {
                EditorGUILayout.HelpBox("⚠️ Target ID không được để trống! Nhập ID của UI element cần highlight.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ Target ID: '{targetIdProp.stringValue}'\nĐảm bảo TutorialTarget component trên UI có cùng ID này.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Message
            EditorGUILayout.LabelField("Message Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(messageProp, new GUIContent("Message", "Tin nhắn hiển thị cho người chơi"));
            
            if (string.IsNullOrEmpty(messageProp.stringValue))
            {
                EditorGUILayout.HelpBox("💡 Thêm tin nhắn hướng dẫn để hiển thị cho người chơi.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Visual Settings
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(focusPaddingProp, new GUIContent("Focus Padding", "Padding xung quanh vùng highlight (px)"));
            EditorGUILayout.PropertyField(blockInputProp, new GUIContent("Block Input", "Chặn input ngoài vùng highlight"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(showHandProp, new GUIContent("Show Hand", "Hiển thị tay chỉ animated"));
            
            if (showHandProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(handOffsetProp, new GUIContent("Hand Offset", "Offset vị trí tay so với target center"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Preview Info
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"Target: {(string.IsNullOrEmpty(targetIdProp.stringValue) ? "Not Set" : targetIdProp.stringValue)}\n" +
                $"Message: {(string.IsNullOrEmpty(messageProp.stringValue) ? "Not Set" : messageProp.stringValue.Substring(0, Mathf.Min(50, messageProp.stringValue.Length)))}", 
                MessageType.None);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Tutorial.Editor
{
    public class TutorialToolsWindow : OdinEditorWindow
    {
        [Title("Actions", bold: true)] [ValueDropdown(nameof(GetAllTypesDropdown))]
        public TutorialType selectedType;

        [Title("Registry", bold: true)] [ShowInInspector] [ReadOnly]
        private string _registryInfo;

        [Title("Status", bold: true)] [ShowInInspector] [ReadOnly] [MultiLineProperty]
        private string _statusInfo;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (selectedType.Equals(default(TutorialType))) selectedType = GetAllTypes().FirstOrDefault();
            RefreshStatus();
        }

        [MenuItem("Tools/Tutorial/Tools Window")]
        private static void OpenWindow()
        {
            ShowWindow();
        }

        public static void ShowWindow()
        {
            var window = GetWindow<TutorialToolsWindow>(false, "Tutorial Tools", true);
            window.minSize = new Vector2(420f, 360f);
            window.RefreshStatus();
        }

        [Button(ButtonSizes.Large)]
        private void ResetSelected()
        {
            TutorialManager.ResetProgress(selectedType);
            RefreshStatus();
        }

        [Button(ButtonSizes.Large)]
        private void CompleteSelected()
        {
            TutorialManager.Complete(selectedType);
            RefreshStatus();
        }

        [Button(ButtonSizes.Medium)]
        private void ResetAll()
        {
            foreach (var type in GetAllTypes()) TutorialManager.ResetProgress(type);
            RefreshStatus();
        }

        [Button(ButtonSizes.Medium)]
        private void CompleteAll()
        {
            foreach (var type in GetAllTypes()) TutorialManager.Complete(type);
            RefreshStatus();
        }

        [Button(ButtonSizes.Medium)]
        private void ReloadSequences()
        {
            TutorialManager.ReloadSequences();
            RefreshStatus();
        }

        [Button(ButtonSizes.Medium)]
        private void SelectRegistryGameObject()
        {
            var registry = TutorialManager.GetRegistry();
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Registry Not Found",
                    "TutorialRegistry not found in scene.\n\n" +
                    "Use: Tools > Tutorial > Registry > Create Registry GameObject",
                    "OK");
                return;
            }

            Selection.activeGameObject = registry.gameObject;
            EditorGUIUtility.PingObject(registry.gameObject);
        }

        private IEnumerable<ValueDropdownItem<TutorialType>> GetAllTypesDropdown()
        {
            return GetAllTypes().Select(type => new ValueDropdownItem<TutorialType>(type.ToString(), type));
        }

        private static IEnumerable<TutorialType> GetAllTypes()
        {
            return (TutorialType[])Enum.GetValues(typeof(TutorialType));
        }

        private void RefreshStatus()
        {
            var registry = TutorialManager.GetRegistry();
            if (registry == null)
                _registryInfo = "Registry: NOT FOUND";
            else
                _registryInfo = $"Registry: ({registry.SequenceCount} sequences)";

            var lines = new List<string>();
            foreach (var type in GetAllTypes())
                lines.Add($"{type}: {(TutorialManager.IsCompleted(type) ? "COMPLETED" : "NOT COMPLETED")}");

            if (TutorialManager.IsActive)
                lines.Add($"Active: {TutorialManager.ActiveType} (Step {TutorialManager.ActiveStepIndex})");
            else
                lines.Add("Active: NONE");

            _statusInfo = string.Join("\n", lines);
            Repaint();
        }
    }
}
#endif
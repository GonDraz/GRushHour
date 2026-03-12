#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tutorial.Editor
{
    /// <summary>
    ///     Editor utilities cho Tutorial System
    /// </summary>
    public static class TutorialEditorTools
    {
        [MenuItem("Tools/Tutorial/Open Tutorial Registry")]
        public static void OpenTutorialRegistry()
        {
            var registry = TutorialManager.GetRegistry();

            if (registry == null)
            {
                Debug.LogError("[Tutorial] TutorialRegistry not found in scene! " +
                               "Create a GameObject and add TutorialRegistry component.");
                EditorUtility.DisplayDialog("Registry Not Found",
                    "TutorialRegistry not found in scene!\n\n" +
                    "Steps to create:\n" +
                    "1. Create GameObject in scene (e.g., 'TutorialRegistry')\n" +
                    "2. Add Component > Tutorial Registry\n" +
                    "3. Drag tutorial sequences into Sequences list\n" +
                    "4. GameObject will auto DontDestroyOnLoad",
                    "OK");
                return;
            }

            Selection.activeGameObject = registry.gameObject;
            EditorGUIUtility.PingObject(registry.gameObject);
            Debug.Log($"[Tutorial] Selected registry GameObject: {registry.gameObject.name}", registry);
        }

        [MenuItem("Tools/Tutorial/Registry/Create Registry GameObject")]
        public static void CreateRegistryGameObject()
        {
            // Check if already exists
            TutorialRegistry existing;
#if UNITY_2023_1_OR_NEWER
            existing = Object.FindFirstObjectByType<TutorialRegistry>();
#else
            existing = Object.FindAnyObjectByType<TutorialRegistry>();
#endif
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                Debug.LogWarning($"[Tutorial] TutorialRegistry already exists on: {existing.gameObject.name}",
                    existing);
                EditorUtility.DisplayDialog("Already Exists",
                    $"TutorialRegistry already exists on GameObject:\n{existing.gameObject.name}\n\nSelected for you.",
                    "OK");
                return;
            }

            // Create new GameObject
            var registryGo = new GameObject("TutorialRegistry");
            registryGo.AddComponent<TutorialRegistry>();

            Selection.activeGameObject = registryGo;
            Undo.RegisterCreatedObjectUndo(registryGo, "Create Tutorial Registry");

            Debug.Log("[Tutorial] Created TutorialRegistry GameObject", registryGo);
            EditorUtility.DisplayDialog("Registry Created",
                "TutorialRegistry GameObject created!\n\n" +
                "Next steps:\n" +
                "1. Drag tutorial sequences into Sequences list\n" +
                "2. GameObject will auto DontDestroyOnLoad on play",
                "OK");
        }

        [MenuItem("Tools/Tutorial/Registry/Validate Registry")]
        public static void ValidateRegistry()
        {
            var registry = TutorialManager.GetRegistry();

            if (registry == null)
            {
                Debug.LogError("[Tutorial] TutorialRegistry not found!");
                return;
            }

            registry.Validate();
            Debug.Log($"[Tutorial] Registry validated: {registry.SequenceCount} sequences", registry);
        }

        [MenuItem("Tools/Tutorial/Registry/Reload Sequences")]
        public static void ReloadSequences()
        {
            TutorialManager.ReloadSequences();
            Debug.Log("[Tutorial] Tutorial sequences reloaded from registry");
        }

        [MenuItem("Tools/Tutorial/Reset All Tutorials")]
        public static void ResetAllTutorials()
        {
            foreach (var type in GetAllTypes()) TutorialManager.ResetProgress(type);

            Debug.Log("[Tutorial] All tutorials have been reset!");
        }

        [MenuItem("Tools/Tutorial/Complete All Tutorials")]
        public static void CompleteAllTutorials()
        {
            foreach (var type in GetAllTypes()) TutorialManager.Complete(type);

            Debug.Log("[Tutorial] All tutorials marked as completed!");
        }

        [MenuItem("Tools/Tutorial/Show Tutorial Status")]
        public static void ShowTutorialStatus()
        {
            Debug.Log("=== Tutorial Status ===");

            var registry = TutorialManager.GetRegistry();
            if (registry != null)
                Debug.Log($"Registry: ({registry.SequenceCount} sequences)");
            else
                Debug.LogWarning("Registry: NOT FOUND");

            foreach (var type in GetAllTypes())
                Debug.Log($"{type}: {(TutorialManager.IsCompleted(type) ? "COMPLETED" : "NOT COMPLETED")}");

            if (TutorialManager.IsActive)
                Debug.Log($"Active Tutorial: {TutorialManager.ActiveType} (Step {TutorialManager.ActiveStepIndex})");
            else
                Debug.Log("No active tutorial");
        }

        private static IEnumerable<TutorialType> GetAllTypes()
        {
            return (TutorialType[])Enum.GetValues(typeof(TutorialType));
        }
    }
}
#endif
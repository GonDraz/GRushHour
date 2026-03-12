using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GonDraz.Scene
{
    [Serializable]
    public class SceneField
    {
        [SerializeField] private Object sceneAsset;
        [SerializeField] private string sceneName = "";

        public string SceneName => sceneName;

        public bool IsLoaded => SceneLoaderService.IsSceneLoaded(SceneName);

        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }

        public async UniTask LoadSceneAsync(LoadSceneMode mode = LoadSceneMode.Single)
        {
            await SceneLoaderService.LoadSceneAsync(SceneName, mode);
        }

        public async UniTask UnloadSceneAsync()
        {
            await SceneLoaderService.UnloadSceneAsync(SceneName);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            var sceneAsset = property.FindPropertyRelative("sceneAsset");
            var sceneName = property.FindPropertyRelative("sceneName");
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (sceneAsset != null)
            {
                sceneAsset.objectReferenceValue = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue,
                    typeof(SceneAsset), false);

                if (sceneAsset.objectReferenceValue)
                    sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset)?.name;
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
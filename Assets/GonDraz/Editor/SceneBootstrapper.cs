using System;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GonDraz.Editor
{
    /// <summary>
    ///     Class that permits auto-loading a bootstrap scene when the editor switches play state. This class is
    ///     initialized when Unity is opened and when scripts are recompiled. This is to be able to subscribe to
    ///     EditorApplication's playModeStateChanged event, which is when we wish to open a new scene.
    /// </summary>
    /// <remarks>
    ///     A critical edge case scenario regarding NetworkManager is accounted for here.
    ///     A NetworkObject's GlobalObjectIdHash value is currently generated in OnValidate() which is invoked during a
    ///     build and when the asset is loaded/viewed in the editor.
    ///     If we were to manually open Bootstrap scene via EditorSceneManager.OpenScene(...) as the editor is exiting play
    ///     mode, Bootstrap scene would be entering play mode within the editor prior to having loaded any assets, meaning
    ///     NetworkManager itself has no entry within the AssetDatabase cache. As a result of this, any referenced Network
    ///     Prefabs wouldn't have any entry either.
    ///     To account for this necessary AssetDatabase step, whenever we're redirecting from a new scene, or a scene
    ///     existing in our EditorBuildSettings, we forcefully stop the editor, open Bootstrap scene, and re-enter play
    ///     mode. This provides the editor the chance to create AssetDatabase cache entries for the Network Prefabs assigned
    ///     to the NetworkManager.
    ///     If we are entering play mode directly from Bootstrap scene, no additional steps need to be taken and the scene
    ///     is loaded normally.
    /// </remarks>
    [InitializeOnLoad]
    public class SceneBootstrapper
    {
        private const string PreviousSceneKey = "PreviousScene";
        private const string ShouldLoadBootstrapSceneKey = "LoadBootstrapScene";

        private const string LoadBootstrapSceneOnPlay = "GonDraz/BootstrapScene/Load Bootstrap Scene On Play";

        private const string DoNotLoadBootstrapSceneOnPlay =
            "GonDraz/BootstrapScene/Don't Load Bootstrap Scene On Play";

        private const string TestRunnerSceneName = "InitTestScene";

        private static bool _restartingToSwitchScene;

        static SceneBootstrapper()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        private static string BootstrapScene => EditorBuildSettings.scenes[0].path;

        // to track where to go back to
        private static string PreviousScene
        {
            get => EditorPrefs.GetString(PreviousSceneKey);
            set => EditorPrefs.SetString(PreviousSceneKey, value);
        }

        private static bool ShouldLoadBootstrapScene
        {
            get
            {
                if (!EditorPrefs.HasKey(ShouldLoadBootstrapSceneKey))
                    EditorPrefs.SetBool(ShouldLoadBootstrapSceneKey, true);

                return EditorPrefs.GetBool(ShouldLoadBootstrapSceneKey, true);
            }
            set => EditorPrefs.SetBool(ShouldLoadBootstrapSceneKey, value);
        }

        [MenuItem(LoadBootstrapSceneOnPlay, true)]
        private static bool ShowLoadBootstrapSceneOnPlay()
        {
            return !ShouldLoadBootstrapScene;
        }

        [MenuItem(LoadBootstrapSceneOnPlay)]
        private static void EnableLoadBootstrapSceneOnPlay()
        {
            ShouldLoadBootstrapScene = true;
        }

        [MenuItem(DoNotLoadBootstrapSceneOnPlay, true)]
        private static bool ShowDoNotLoadBootstrapSceneOnPlay()
        {
            return ShouldLoadBootstrapScene;
        }

        [MenuItem(DoNotLoadBootstrapSceneOnPlay)]
        private static void DisableDoNotLoadBootstrapSceneOnPlay()
        {
            ShouldLoadBootstrapScene = false;
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (IsTestRunnerActive()) return;

            if (!ShouldLoadBootstrapScene) return;

            if (_restartingToSwitchScene)
            {
                if (playModeStateChange == PlayModeStateChange.EnteredPlayMode)
                    // for some reason there's multiple start and stops events happening while restarting the editor playmode. We're making sure to
                    // set stoppingAndStarting only when we're done and we've entered playmode. This way we won't corrupt "activeScene" with the multiple
                    // start and stop and will be able to return to the scene we were editing at first
                    _restartingToSwitchScene = false;
                return;
            }

            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                // cache previous scene so we return to this scene after play session, if possible
                PreviousScene = EditorSceneManager.GetActiveScene().path;

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // user either hit "Save" or "Don't Save"; open bootstrap scene

                    if (!string.IsNullOrEmpty(BootstrapScene) &&
                        Array.Exists(EditorBuildSettings.scenes, scene => scene.path == BootstrapScene))
                    {
                        var activeScene = EditorSceneManager.GetActiveScene();

                        _restartingToSwitchScene = activeScene.path == string.Empty ||
                                                   !BootstrapScene.Contains(activeScene.path);

                        // we only manually inject Bootstrap scene if we are in a blank empty scene,
                        // or if the active scene is not already BootstrapScene
                        if (_restartingToSwitchScene)
                        {
                            EditorApplication.isPlaying = false;

                            // scene is included in build settings; open it
                            EditorSceneManager.OpenScene(BootstrapScene);

                            EditorApplication.isPlaying = true;
                        }
                    }
                }
                else
                {
                    // user either hit "Cancel" or exited window; don't open bootstrap scene & return to editor
                    EditorApplication.isPlaying = false;
                }
            }
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (!string.IsNullOrEmpty(PreviousScene)) EditorSceneManager.OpenScene(PreviousScene);
            }
        }

        private static bool IsTestRunnerActive()
        {
            return EditorSceneManager.GetActiveScene().name.StartsWith(TestRunnerSceneName);
        }
    }
}
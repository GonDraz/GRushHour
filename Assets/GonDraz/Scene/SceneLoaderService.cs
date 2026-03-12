using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace GonDraz.Scene
{
    public static class SceneLoaderService
    {
        private static readonly Dictionary<string, SceneStatus> SceneStates = new();
        private static readonly Queue<Func<UniTask>> SceneTaskQueue = new();
        private static readonly SemaphoreSlim ProcessLock = new(1, 1);

        public static async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!SceneStates.TryGetValue(sceneName, out var status) || status == SceneStatus.Unloaded)
            {
                SceneStates[sceneName] = SceneStatus.Loading;
                SceneTaskQueue.Enqueue(() => Load(sceneName, mode));
                await ProcessQueue();
            }
        }

        public static async UniTask UnloadSceneAsync(string sceneName)
        {
            if (SceneStates.TryGetValue(sceneName, out var status) && status == SceneStatus.Loaded)
            {
                SceneStates[sceneName] = SceneStatus.Unloading;
                SceneTaskQueue.Enqueue(() => Unload(sceneName));
                await ProcessQueue();
            }
        }

        private static async UniTask Load(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                var op = SceneManager.LoadSceneAsync(sceneName, mode);
                if (op != null) await op.ToUniTask();
            }

            SceneStates[sceneName] = SceneStatus.Loaded;
        }

        private static async UniTask Unload(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(sceneName);
                if (op != null) await op.ToUniTask();
            }

            SceneStates[sceneName] = SceneStatus.Unloaded;
        }

        private static async UniTask ProcessQueue()
        {
            await ProcessLock.WaitAsync();

            try
            {
                while (SceneTaskQueue.Count > 0)
                {
                    var taskFunc = SceneTaskQueue.Dequeue();
                    await taskFunc();
                    await UniTask.Yield();
                }
            }
            finally
            {
                ProcessLock.Release();
            }
        }

        public static bool IsSceneLoaded(string sceneName)
        {
            return SceneStates.TryGetValue(sceneName, out var state) && state == SceneStatus.Loaded;
        }

        private enum SceneStatus
        {
            Unloaded,
            Loading,
            Loaded,
            Unloading
        }
    }
}
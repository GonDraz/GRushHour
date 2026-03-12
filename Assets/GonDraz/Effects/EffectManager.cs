using System;
using System.Collections.Generic;
using GonDraz.ObjectPool;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GonDraz.Effects
{
    /// <summary>
    ///     Static manager for effects, follows GonDraz PoolManager pattern
    /// </summary>
    public static partial class EffectManager
    {
        private static readonly Transform EffectRoot;
        private static EffectRegistry _effectRegistry;
        private static readonly Dictionary<string, BaseEffect> EffectPrefabs = new();

        static EffectManager()
        {
            var root = new GameObject("[EffectManager]");
            Object.DontDestroyOnLoad(root);
            EffectRoot = root.transform;

            Debug.Log("EffectManager initialized");
        }

        /// <summary>
        ///     Register effect prefab with key
        /// </summary>
        public static void RegisterEffect(string key, BaseEffect prefab)
        {
            if (!EffectPrefabs.ContainsKey(key))
                EffectPrefabs[key] = prefab;
            else
                Debug.LogWarning($"Effect with key '{key}' already registered");
        }

        /// <summary>
        ///     Play effect from prefab
        /// </summary>
        private static T PlayEffect<T>(BaseEffect effectPrefab, Vector3 position, Transform parent = null,
            Action onComplete = null) where T : BaseEffect
        {
            if (!effectPrefab)
            {
                Debug.LogError("Effect prefab is null");
                return null;
            }

            var effectObj = PoolManager.Get<T>(effectPrefab.gameObject, parent ?? EffectRoot);

            if (!effectObj)
            {
                Debug.LogError($"Failed to get effect from pool: {effectPrefab.name}");
                return null;
            }

            effectObj.transform.position = position;
            effectObj.Play(onComplete);

            return effectObj;
        }

        /// <summary>
        ///     Play effect from registered key
        /// </summary>
        public static T PlayEffect<T>(string effectKey, Vector3 position, Transform parent = null,
            Action onComplete = null) where T : BaseEffect
        {
            if (!EffectPrefabs.TryGetValue(effectKey, out var prefab))
            {
                Debug.LogError($"Effect with key '{effectKey}' not found. Did you register it?");
                return null;
            }

            return PlayEffect<T>(prefab, position, parent, onComplete);
        }

        /// <summary>
        ///     Play UI effect
        /// </summary>
        private static T PlayUIEffect<T>(BaseUIEffect effectPrefab, Transform parent,
            Vector2 anchoredPosition = default,
            Action onComplete = null) where T : BaseUIEffect
        {
            if (!effectPrefab)
            {
                Debug.LogError("UI Effect prefab is null");
                return null;
            }

            var effectObj = PoolManager.Get<T>(effectPrefab.gameObject, parent);

            if (!effectObj) return null;

            var rectTransform = effectObj.GetTargetRectTransform();
            if (rectTransform)
            {
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.localScale = Vector3.one;
            }

            effectObj.Play(onComplete);
            return effectObj;
        }

        /// <summary>
        ///     Play UI effect from registered key
        /// </summary>
        public static T PlayUIEffect<T>(string effectKey, Transform parent,
            Vector2 anchoredPosition = default,
            Action onComplete = null) where T : BaseUIEffect
        {
            if (!EffectPrefabs.TryGetValue(effectKey, out var prefab))
            {
                Debug.LogError($"UI Effect with key '{effectKey}' not found");
                return null;
            }

            return PlayUIEffect<T>(prefab as BaseUIEffect, parent, anchoredPosition, onComplete);
        }

        /// <summary>
        ///     Preload effect into pool
        /// </summary>
        public static void PreloadEffect(BaseEffect effectPrefab, int count = 5)
        {
            PoolManager.GetPool(effectPrefab.gameObject, count);
        }

        /// <summary>
        ///     Clear all registered effects
        /// </summary>
        public static void ClearRegistry()
        {
            EffectPrefabs.Clear();
        }

        public static void SetGetEffectRegistry(EffectRegistry registry)
        {
            _effectRegistry = registry;
        }

        public static Canvas GetEffectCanvas()
        {
            return _effectRegistry ? _effectRegistry.EffectCanvas : null;
        }

        public static RectTransform GetEffectRect()
        {
            return _effectRegistry.CanvasRectTransform;
        }
    }
}
using System;
using GonDraz.Base;
using GonDraz.Extensions;
using UnityEngine;

namespace GonDraz.Effects
{
    /// <summary>
    ///     Auto-register all effects on game start
    /// </summary>
    public sealed class EffectRegistry : BaseBehaviour
    {
        [Header("Effect Configs")] [SerializeField]
        private Canvas canvas;

        [SerializeField] private EffectConfig[] effectPrefabs;

        private RectTransform _canvasRectTransform;

        public Canvas EffectCanvas => canvas;

        public RectTransform CanvasRectTransform
        {
            get
            {
                if (!_canvasRectTransform && canvas)
                    _canvasRectTransform = canvas.GetComponent<RectTransform>();
                return _canvasRectTransform;
            }
        }

        private void Start()
        {
            EffectManager.SetGetEffectRegistry(this);
            RegisterAllEffects();
            PreloadAllEffects();
        }

        private void RegisterAllEffects()
        {
            foreach (var effectConfig in effectPrefabs)
                if (effectConfig.prefab && !effectConfig.key.IsNullOrEmpty())
                    EffectManager.RegisterEffect(effectConfig.key, effectConfig.prefab);

            Debug.Log($"Registered {effectPrefabs.Length} effects");
        }

        private void PreloadAllEffects()
        {
            foreach (var effectConfig in effectPrefabs)
                if (effectConfig.prefab && !effectConfig.key.IsNullOrEmpty())
                    EffectManager.PreloadEffect(effectConfig.prefab, effectConfig.preloadCount);
        }

        [Serializable]
        public class EffectConfig
        {
            public string key;
            public BaseEffect prefab;
            public int preloadCount;
        }
    }
}
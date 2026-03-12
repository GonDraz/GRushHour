using System;
using GonDraz.Base;
using GonDraz.Events;
using GonDraz.ObjectPool;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GonDraz.Effects
{
    /// <summary>
    ///     Base class for all effects, inherits from GonDraz BaseBehaviour
    /// </summary>
    public abstract class BaseEffect : BaseBehaviour, IPoolable
    {
        [Header("Effect Settings")] [SerializeField]
        protected float duration = 1f;

        [SerializeField] protected Ease easeType = Ease.OutQuad;
        [SerializeField] protected bool autoReturnToPool = true;

        private GEvent _onCompleteCallback = new("Effect-OnComplete");

        protected Sequence CurrentSequence;

        public bool IsPlaying { get; protected set; }
        private bool IsSetup { get; set; }
        public float Progress { get; protected set; }

        protected override void OnDestroy()
        {
            CurrentSequence.Stop();
            base.OnDestroy();
        }

        public virtual void OnGetFromPool()
        {
            Setup();
            IsPlaying = false;
            Progress = 0;
        }

        public virtual void OnReturnToPool()
        {
            Stop();
        }

        private void Setup()
        {
            if (IsSetup) return;
            IsSetup = true;
            OnSetup();
        }

        public void Play(Action onComplete = null)
        {
            if (IsPlaying)
                Stop();

            _onCompleteCallback = onComplete;
            IsPlaying = true;
            Progress = 0;

            OnPlay();
        }

        private void Stop()
        {
            if (!IsPlaying) return;

            IsPlaying = false;
            CurrentSequence.Stop();
            OnStop();

            if (autoReturnToPool) this.Return();
        }

        protected void CompleteEffect()
        {
            IsPlaying = false;
            Progress = 1f;
            OnComplete();
            _onCompleteCallback?.Invoke();

            if (autoReturnToPool) this.Return();
        }


        // Abstract methods - Subclass implements
        [Button]
        protected abstract void OnPlay();

        [Button]
        protected abstract void OnStop();

        protected virtual void OnComplete()
        {
        }

        protected virtual void OnSetup()
        {
        }
    }
}
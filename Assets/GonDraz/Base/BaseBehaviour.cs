using UnityEngine;

namespace GonDraz.Base
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        private bool _isSubscribe;

        protected virtual void OnEnable()
        {
            if (!SubscribeUsingOnEnable() || _isSubscribe) return;
            Subscribe();
        }

        protected virtual void OnDisable()
        {
            if (!UnsubscribeUsingOnDisable() || !_isSubscribe) return;
            Unsubscribe();
        }

        protected virtual void OnDestroy()
        {
            Unsubscribe();
        }

        public virtual bool SubscribeUsingOnEnable()
        {
            return false;
        }

        public virtual bool UnsubscribeUsingOnDisable()
        {
            return false;
        }

        public virtual void Subscribe()
        {
            if (_isSubscribe) return;
            _isSubscribe = true;
        }

        public virtual void Unsubscribe()
        {
            if (!_isSubscribe) return;
            _isSubscribe = false;
        }

        protected virtual void SubscribesChild()
        {
            SubscribesChild<BaseBehaviour>();
        }

        protected virtual void SubscribesChild<T>() where T : BaseBehaviour
        {
            var childArray = GetComponentsInChildren<T>();

            foreach (var child in childArray) child.Subscribe();
        }

        protected virtual void UnsubscribeChild()
        {
            UnsubscribeChild<BaseBehaviour>();
        }

        protected virtual void UnsubscribeChild<T>() where T : BaseBehaviour
        {
            var childArray = GetComponentsInChildren<T>();

            foreach (var child in childArray) child.Unsubscribe();
        }

        protected virtual void Active()
        {
            gameObject.SetActive(true);
        }

        protected virtual void Inactive()
        {
            gameObject.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using GonDraz.Extensions;
using UnityEngine;

namespace GonDraz.ObjectPool
{
    public class ObjectPool
    {
        private readonly HashSet<GameObject> _activeObjects = new();
        private readonly HashSet<GameObject> _inPool = new();
        private readonly Stack<GameObject> _pool = new();

        private readonly GameObject _prefab;
        private readonly Transform _root;

        public ObjectPool(GameObject prefab, int preloadCount = 0, Transform root = null)
        {
            _prefab = prefab;
            _root = root;

            for (var i = 0; i < preloadCount; i++)
            {
                var obj = CreateNew();
                Return(obj);
            }
        }

        private GameObject CreateNew()
        {
            var obj = Object.Instantiate(_prefab, _root);
            obj.name = $"{_prefab.name}";
            obj.SetActive(false);

            var member = obj.GetOrAddComponent<PoolMember>();

            member.Pool = this;
            member.IsInPool = false;

            return obj;
        }

        public GameObject Get(Transform parent)
        {
            GameObject obj;

            while (_pool.Count > 0)
            {
                obj = _pool.Pop();
                _inPool.Remove(obj);

                if (!obj) continue;
                Activate(obj, parent);
                return obj;
            }

            obj = CreateNew();
            Activate(obj, parent);
            return obj;
        } // ReSharper disable Unity.PerformanceAnalysis
        public T Get<T>(Transform parent) where T : Component
        {
            var obj = Get(parent);
            var comp = obj.GetComponent<T>();

            if (!comp)
                Debug.LogError($"{_prefab.name} does not contain component {typeof(T)}");

            return comp;
        }

        public void Return(GameObject obj)
        {
            if (!obj || _inPool.Contains(obj))
                return;

            Deactivate(obj);

            _inPool.Add(obj);
            _pool.Push(obj);
        }

        internal void NotifyDestroyed(GameObject obj)
        {
            _inPool.Remove(obj);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Activate(GameObject obj, Transform parent = null)
        {
            _activeObjects.Add(obj);
            var member = obj.GetComponent<PoolMember>();
            member.IsInPool = false;

            obj.SetActive(true);
            obj.transform.SetParent(parent, false);
            obj.GetComponent<IPoolable>()?.OnGetFromPool();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Deactivate(GameObject obj)
        {
            _activeObjects.Remove(obj);
            obj.GetComponent<IPoolable>()?.OnReturnToPool();

            var member = obj.GetComponent<PoolMember>();
            member.IsInPool = true;
            if (obj.activeSelf) obj.transform.SetParent(_root, false);
            if (obj.activeSelf) obj.SetActive(false);
        }

        public void ReturnAll()
        {
            var activeList = new List<GameObject>(_activeObjects);
            foreach (var obj in activeList) Return(obj);
        }
    }
}
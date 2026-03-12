using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GonDraz.ObjectPool
{
    public static class PoolManager
    {
        private static readonly Dictionary<GameObject, ObjectPool> Pools = new();
        private static readonly Transform PoolRoot;

        static PoolManager()
        {
            var root = new GameObject("PoolManager");
            Object.DontDestroyOnLoad(root);
            PoolRoot = root.transform;
        }

        public static ObjectPool GetPool(GameObject prefab, int preload = 0)
        {
            if (Pools.TryGetValue(prefab, out var pool)) return pool;
            var root = new GameObject("Pool " + prefab.name);
            root.transform.SetParent(PoolRoot);
            pool = new ObjectPool(prefab, preload, root.transform);
            Pools.Add(prefab, pool);

            return pool;
        }

        public static GameObject Get(GameObject prefab, Transform parent = null, int preload = 0)
        {
            return GetPool(prefab, preload).Get(parent);
        }

        public static T Get<T>(GameObject prefab, Transform parent = null, int preload = 0)
            where T : Component, IPoolable
        {
            return GetPool(prefab, preload).Get<T>(parent);
        }

        public static void Return(this IPoolable obj)
        {
            if (obj is Component comp) ReturnObject(comp.gameObject);
        }

        [CanBeNull]
        public static PoolMember GetPoolMember(this IPoolable obj)
        {
            if (obj is Component comp) return comp.GetComponent<PoolMember>();
            return null;
        }


        public static void ReturnObject(GameObject obj)
        {
            if (!obj) return;

            var member = obj.GetComponent<PoolMember>();
            if (!member)
            {
                Object.Destroy(obj);
                return;
            }

            member.ReturnToPool();
        }

        public static void ReturnAll()
        {
            foreach (var pool in Pools.Values) pool.ReturnAll();
        }
    }
}
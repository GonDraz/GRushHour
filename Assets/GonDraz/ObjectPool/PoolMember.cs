using UnityEngine;

namespace GonDraz.ObjectPool
{
    public class PoolMember : MonoBehaviour
    {
        internal ObjectPool Pool { get; set; }
        internal bool IsInPool { get; set; }

        private void OnDestroy()
        {
            Pool?.NotifyDestroyed(gameObject);
        }

        public void ReturnToPool()
        {
            if (Pool == null || IsInPool)
                return;

            Pool.Return(gameObject);
        }
    }
}
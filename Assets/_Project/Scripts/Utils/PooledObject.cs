using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class PooledObject : MonoBehaviour
    {
        private SimpleObjectPool owner;
        public void SetOwner(SimpleObjectPool pool) => owner = pool;
        public void ReturnToPool()
        {
            if (owner != null) owner.Return(gameObject);
            else Destroy(gameObject);
        }
    }
}

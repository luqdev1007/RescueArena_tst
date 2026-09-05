using UnityEngine;

namespace WebGLRescueArena
{
    [RequireComponent(typeof(PooledObject))]
    public sealed class PooledEffect : MonoBehaviour
    {
        private PooledObject pooled;
        private void Awake() => pooled = GetComponent<PooledObject>();
        private void OnDisable() => pooled.ReturnToPool();
    }
}
using UnityEngine;

namespace WebGLRescueArena
{
    [RequireComponent(typeof(PooledObject))]
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private GameObject deathEffectPrefab;
        private PooledObject pooled;
        private SimpleObjectPool deathEffectPool;
        private int currentHealth;
        private bool dead;
        private void Awake() => pooled = GetComponent<PooledObject>();
        private void OnEnable() { currentHealth = maxHealth; dead = false; }
        public void SetDeathEffectPool(SimpleObjectPool pool) => deathEffectPool = pool;
        public void TakeDamage(int damage)
        {
            if (dead) return;
            currentHealth -= damage;
            if (currentHealth > 0) return;
            dead = true;
            if (deathEffectPool != null) deathEffectPool.Take(transform.position, Quaternion.identity);
            else if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            GameEvents.RaiseEnemyKilled(scoreValue);
            pooled.ReturnToPool();
        }
    }
}
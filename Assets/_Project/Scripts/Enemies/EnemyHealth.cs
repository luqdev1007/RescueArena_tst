using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private SimpleObjectPool deathEffectPool;

        private int currentHealth;
        private bool dead;

        private void Awake() => currentHealth = maxHealth;

        private void OnEnable() 
        { 
            currentHealth = maxHealth; dead = false;
        }

        public void TakeDamage(int damage)
        {
            if (dead) 
                return;

            currentHealth -= damage;

            if (currentHealth > 0) 
                return;

            dead = true;

            if (deathEffectPool != null) 
                deathEffectPool.Take(transform.position, Quaternion.identity);

            GameEvents.RaiseEnemyKilled(scoreValue);
            Destroy(gameObject);
        }
    }
}
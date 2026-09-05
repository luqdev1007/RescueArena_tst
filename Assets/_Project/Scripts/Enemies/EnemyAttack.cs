using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private int damage = 8;
        [SerializeField] private float attackRange = 1.35f;
        [SerializeField] private float attackCooldown = 0.9f;
        private float nextAttack;
        private PlayerHealth cachedHealth;
        private Transform cachedTarget;
        private void OnEnable() { nextAttack = 0f; cachedTarget = null; cachedHealth = null; }
        public void Tick(Transform target, float distance)
        {
            if (target == null || Time.time < nextAttack || distance > attackRange) return;
            if (target != cachedTarget)
            {
                cachedTarget = target;
                cachedHealth = target.GetComponent<PlayerHealth>();
            }
            if (cachedHealth == null) return;
            nextAttack = Time.time + attackCooldown;
            cachedHealth.TakeDamage(damage);
        }
    }
}
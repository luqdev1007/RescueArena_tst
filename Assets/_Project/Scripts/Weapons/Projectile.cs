using UnityEngine;

namespace WebGLRescueArena
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 2.5f;
        [SerializeField] private GameObject fallbackImpactEffect;

        private Rigidbody body;
        private SimpleObjectPool ownerPool;
        private SimpleObjectPool impactPool;

        private int damage;
        private float despawnTime;
        private bool consumed;

        private void Awake() => body = GetComponent<Rigidbody>();

        public void Launch(float speed, int damageValue, SimpleObjectPool pool, SimpleObjectPool impacts)
        {
            damage = damageValue;
            ownerPool = pool;
            impactPool = impacts;
            consumed = false;
            despawnTime = Time.time + lifetime;
            body.linearVelocity = transform.forward * speed;
        }

        private void Update()
        {
            if (!consumed && Time.time >= despawnTime) 
                Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed) 
                return;
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            if (enemy != null) 
                enemy.TakeDamage(damage);

            SpawnImpact();
            Despawn();
        }

        private void SpawnImpact()
        {
            if (impactPool != null)
            {
                impactPool.Take(transform.position, Quaternion.identity);
                return;
            }

            if (fallbackImpactEffect != null) 
                Instantiate(fallbackImpactEffect, transform.position, Quaternion.identity);
        }

        private void Despawn()
        {
            consumed = true;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            if (ownerPool != null) 
                ownerPool.Return(gameObject);
            else 
                Destroy(gameObject);
        }
    }
}
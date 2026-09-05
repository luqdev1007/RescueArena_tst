using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private SimpleObjectPool projectilePool;
        [SerializeField] private SimpleObjectPool impactPool;
        [SerializeField] private Transform firePoint;

        [SerializeField] private float fireRate = 0.12f;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private int damage = 10;

        private float nextShotTime;

        private void Update()
        {
            if (!input.FireHeld || Time.time < nextShotTime) 
                return;

            nextShotTime = Time.time + fireRate;
            GameObject instance = projectilePool.Take(firePoint.position, firePoint.rotation);
            Projectile projectile = instance.GetComponent<Projectile>();

            if (projectile != null) 
                projectile.Launch(projectileSpeed, damage, projectilePool, impactPool);
        }
    }
}
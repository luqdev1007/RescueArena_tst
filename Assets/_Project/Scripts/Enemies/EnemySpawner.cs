using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private SimpleObjectPool enemyPool;
        [SerializeField] private SimpleObjectPool deathEffectPool;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int startingEnemies = 12;
        [SerializeField] private int normalCap = 55;
        [SerializeField] private int stressCap = 140;
        [SerializeField] private float spawnInterval = 1f;
        private readonly List<GameObject> active = new List<GameObject>(256);
        private int cap = -1;
        private float waitInterval = -1f;
        private WaitForSeconds wait;
        public int ActiveEnemyCount => active.Count;
        private void Start()
        {
            if (cap < 0) cap = normalCap;
            StartCoroutine(SpawnLoop());
            for (int index = 0; index < startingEnemies; index++) SpawnEnemy();
        }
        public void EnableStressMode() { cap = stressCap; spawnInterval = 0.12f; }
        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                Prune();
                if (active.Count < cap) SpawnEnemy();
                if (waitInterval != spawnInterval)
                {
                    waitInterval = spawnInterval;
                    wait = new WaitForSeconds(waitInterval);
                }
                yield return wait;
            }
        }
        private void Prune()
        {
            for (int index = active.Count - 1; index >= 0; index--)
            {
                GameObject enemy = active[index];
                if (enemy == null || !enemy.activeSelf) active.RemoveAt(index);
            }
        }
        private void SpawnEnemy()
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = enemyPool.Take(point.position, Quaternion.identity);
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null) health.SetDeathEffectPool(deathEffectPool);
            active.Add(enemy);
        }
    }
}
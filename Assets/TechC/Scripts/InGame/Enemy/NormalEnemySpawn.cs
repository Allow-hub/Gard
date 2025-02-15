using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class NormalEnemySpawn : MonoBehaviour
    {
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int maxEnemies = 10; // 最大数
        [SerializeField] private float spawnRadius = 5f; // 半径5
        [SerializeField] private float spawnInterval = 3f; // 3秒ごと

        private List<GameObject> activeEnemies = new List<GameObject>(); // 現在の敵リスト

        private void Start()
        {
            StartCoroutine(SpawnEnemies());
        }

        private IEnumerator SpawnEnemies()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);

                if (activeEnemies.Count < maxEnemies)
                {
                    SpawnEnemy();
                }
            }
        }

        private void SpawnEnemy()
        {
            if (objectPool == null)
                objectPool = GameManager.I.GetEnemyPool();

            Vector3 spawnPosition = transform.position + (Random.insideUnitSphere * spawnRadius);

            GameObject enemy = objectPool.GetObject(enemyPrefab);
            if (enemy != null)
            {
                enemy.transform.position = spawnPosition;
                enemy.SetActive(true);
                activeEnemies.Add(enemy);
            }
        }

        public void RemoveEnemy(GameObject enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                objectPool.ReturnObject(enemy);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red; // 赤色の円を表示
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }

    }
}

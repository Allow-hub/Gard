using System.Collections;
using System.Collections.Generic;
using TechC.Interface;
using TechC.Status;
using UnityEngine;

namespace TechC.Enemy
{
    public class NormalEnemy : MonoBehaviour, IDamageable
    {
        [Header("Reference")]
        [SerializeField] private EnemyStatus enemyStatus;
        [SerializeField] private GameObject[] enemyObjects;
        private ObjectPool objectPool;

        [Header("Parameter")]
        [SerializeField] private string enemyName;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject explosion;
        [SerializeField] private int addPoint = 100;
        [SerializeField] private Vector3 minSize;
        [SerializeField] private Vector3 maxSize;
        [SerializeField] private Vector2 intervalRange = new Vector2(5, 10);
        [SerializeField] private float distance = 200;//playerとの距離により攻撃場所を変える
        [SerializeField] private float bulletSpeed = 100f;
        [SerializeField] private Vector3 homePos;
        
        private float currentInterval;
        private float elapsedTime;
        private int currentHealth;
        private bool canMove = false;

        private void OnValidate()
        {
            enemyName = enemyStatus.enemyName;
            int n = transform.childCount;
            enemyObjects = new GameObject[n];
            for (int i = 0; i < n; i++)
            {
                enemyObjects[i] = transform.GetChild(i).gameObject;
            }
        }

        private void Start()
        {
            objectPool = GameManager.I.GetEnemyPool();
        }

        private void OnEnable()
        {
            Init();
        }
        private void OnDisable()
        {
            elapsedTime = 0;
            if (objectPool == null || explosion == null) return;
            var obj = objectPool.GetObject(explosion);
            obj.transform.position = gameObject.transform.position;
        }
        private void Init()
        {
            // localScale のサイズをランダム化
            gameObject.transform.localScale = Vector3.Lerp(minSize, maxSize, Random.value);
            currentHealth = enemyStatus.maxHealth;
            currentInterval = Random.Range(intervalRange.x, intervalRange.y);
            // ランダムに1つの子オブジェクトを有効化
            foreach (var obj in enemyObjects)
                obj.SetActive(false);

            int rand = Random.Range(0, enemyObjects.Length);
            enemyObjects[rand].SetActive(true);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime > currentInterval )
            {
                Vector3 targetPos;
                if(GameManager.I.CalcDistanceFromPlayer(gameObject.transform.position)<=distance)
                {
                    targetPos = GameManager.I.GetPlayerTransform().position;
                }
                else
                {
                    targetPos = homePos;
                }

                ShootBullet(targetPos);

                currentInterval = Random.Range(intervalRange.x, intervalRange.y);
                elapsedTime = 0; 
            }
            if (!canMove) return;
        }
        private void ShootBullet(Vector3 targetPos)
        {
            if (bulletPrefab == null || objectPool == null) return;

            // プールから弾を取得
            GameObject bullet = objectPool.GetObject(bulletPrefab);
            bullet.transform.position = transform.position; // 敵の位置から発射
            bullet.transform.LookAt(targetPos); // 弾の向きをターゲット方向に設定

            // Rigidbodyを取得して前進させる
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                rb.velocity = direction * bulletSpeed;
            }
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            // ヒットエフェクトを追加する場合はここで処理
            OnHitEffect();

            if (currentHealth <= 0)
            {
                Death();
            }
        }

        // ヒット時のエフェクト処理（拡張用）
        private void OnHitEffect()
        {
            // TODO: ヒット時のエフェクトを追加する
        }

        [ContextMenu("Trigger Death")]
        public void Death()
        {
            if (objectPool == null) return;

  
            // スコア加算
            GameManager.I.AddPoint(addPoint);

            // オブジェクトをプールに戻す
            objectPool.ReturnObject(gameObject);
        }

        public void SetCanMove() => canMove = !canMove;
        public bool GetCanMove() => canMove;
    }
}

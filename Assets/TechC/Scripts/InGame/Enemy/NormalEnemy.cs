using System.Collections;
using System.Collections.Generic;
using TechC.Interface;
using TechC.Status;
using UnityEngine;

namespace TechC.Enemy
{
    public class NormalEnemy : MonoBehaviour ,IDamageable
    {
        [Header("Reference")]
        [SerializeField] private EnemyStatus enemyStatus;
        private ObjectPool objectPool;

        [Header("Parameter")]
        [SerializeField] private  string enemyName;
        private int currentHealth;

        private void OnValidate()
        {
            enemyName = enemyStatus.enemyName;
        }

        private void Start()
        {
            //自オブジェクトの親はNormalEnemyでその親はEnemyPool
            objectPool = transform.parent?.parent?.GetComponent<ObjectPool>();
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            currentHealth = enemyStatus.maxHealth; 
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Death();
            }
        }
        [ContextMenu("Trigger Death")]

        public void Death()
        {
            if (objectPool == null) return;
            objectPool.ReturnObject(gameObject);
        }
    }
}

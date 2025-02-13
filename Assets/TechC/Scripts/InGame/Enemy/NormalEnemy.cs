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
        [SerializeField] private GameObject explosion;
        [SerializeField] private int addPoint = 100;
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
            //自オブジェクトの親はNormalEnemyでその親はEnemyPool
            objectPool = transform.parent?.parent?.GetComponent<ObjectPool>();
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            if (objectPool == null || explosion == null) return;
            var obj = objectPool.GetObject(explosion);
            obj.transform.position = gameObject.transform.position;
        }

        private void Init()
        {
            currentHealth = enemyStatus.maxHealth;
            foreach (var obj in enemyObjects)
                obj.SetActive(false);
            int rand = Random.Range(0, transform.childCount);
            enemyObjects[rand].SetActive(true);

        }


        private void Update()
        {
            if (!canMove) return;
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
            GameManager.I.AddPoint(addPoint);
            objectPool.ReturnObject(gameObject);
        }

        //private void OnCollisionEnter(Collision collision)
        //{
        //    if (collision.gameObject.CompareTag("Weapon"))
        //    {

        //    }
        //}
        public void SetCanMove() => canMove = !canMove;
        public bool GetCanMove() => canMove;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechC;
using TechC.Interface;

namespace TechC
{
    public class BulletCol : MonoBehaviour
    {
        [SerializeField] private float activeDuration = 5f;
        [SerializeField] private GameObject[] explosionPrefabs;
        [SerializeField] private GameObject soundPrefab;
        [SerializeField] private int initdamage;

        private int currentDamage;
        private ObjectPool enemyPool;
        private ObjectPool soundPool;
        private float elapsedTime = 0;

        private void OnDisable()
        {
            elapsedTime = 0;
        }

        private void Awake()
        {
            currentDamage = initdamage;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime > activeDuration)
            {
                if (enemyPool == null || soundPool == null)
                {
                    enemyPool = GameManager.I.GetEnemyPool();
                    soundPool = GameManager.I.GetSoundPool();
                }
                enemyPool.ReturnObject(gameObject);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
            if(enemyPool==null || soundPool == null)
            {
                enemyPool = GameManager.I.GetEnemyPool();
                soundPool = GameManager.I.GetSoundPool();   
            }
            uint rand =(uint)Random.Range(0, explosionPrefabs.Length);
            var explosionObj = enemyPool.GetObject(explosionPrefabs[rand]);
            Vector3 hitPosition = other.ClosestPoint(transform.position);
            explosionObj.transform.position = hitPosition;
            var se = soundPool.GetObject(soundPrefab);
            se.transform.position = hitPosition;
            this.DelayMethod(0.1f,()=> enemyPool.ReturnObject(gameObject));
            IDamageable damageable = other.gameObject.transform.parent?.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // ダメージを与える
                damageable.TakeDamage(currentDamage);
            }
        }
    }
}

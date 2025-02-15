using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class EnemyBullet : MonoBehaviour
    {
        [SerializeField] private GameObject hitObj;
        [SerializeField] private int damage = 10;
        private ObjectPool objectPool;

        // レイヤーマスクを設定
        [SerializeField] private LayerMask grappleableLayer;
        [SerializeField] private LayerMask playerLayer;

        private void Start()
        {
            objectPool = GameManager.I.GetEnemyPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            int otherLayer = other.gameObject.layer; // 衝突したオブジェクトのレイヤー

            if ((grappleableLayer & (1 << otherLayer)) != 0) // Grappleable レイヤーの判定
            {
                HandleHit(other);
                GameManager.I.AddHomeHp(-damage);
            }
            else if ((playerLayer & (1 << otherLayer)) != 0) // Player レイヤーの判定
            {
                HandleHit(other);
                GameManager.I.AddPlayerHp(-damage);
            }
        }

        private void HandleHit(Collider other)
        {
            var obj = objectPool.GetObject(hitObj);
            Vector3 hitPosition = other.ClosestPoint(transform.position);
            obj.transform.position = hitPosition;
            objectPool.ReturnObject(gameObject);
        }
    }
}

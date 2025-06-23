using System.Collections;
using System.Collections.Generic;
using TechC.Interface;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class Turret : MonoBehaviour, IIntaractable
    {
        [SerializeField] private Transform turret;
        [SerializeField] private float bulletSpeed = 10;
        [SerializeField] private Transform shotPos;
        [SerializeField] private int maxLevel = 5;           // 最大レベル
        [SerializeField] private float initRate = 3f;        // 初期発射レート
        [SerializeField] private float rateMagnification = 0.2f; // レベルアップ時の発射レート増加倍率
        [SerializeField] private GameObject bulletPrefab;    // 弾のプレハブ
        [SerializeField] private float detectionRange = 10f; // 敵の検出範囲
        [SerializeField] private LayerMask targetLayer;      // 検出対象のレイヤー
        [SerializeField] private int changeTargetCount=5;
        [SerializeField] private TextMeshProUGUI pointText;
        [SerializeField] private int initNeedPoint;
        [SerializeField] private float pointMagnification = 0.2f;

        private int currentNeedPoint;
        private float currentRate;   // 現在の発射レート
        private uint currentLevel;   // 現在のレベル
        private const uint initLevel = 1; // 初期レベル
        private ObjectPool bulletPool;    // 弾のオブジェクトプール
        private float elapsedTime = 0;    // 経過時間
        private Transform target;         // 現在のターゲット
        private int fireCount;
        private void Start()
        {
            currentLevel = initLevel;
            currentRate = initRate;
            currentNeedPoint = initNeedPoint;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            // ターゲットがいなければ探す
            if (target == null || target.transform.parent == null || !target.transform.parent.gameObject.activeSelf)
            {
                DetectTarget(); // 非アクティブならターゲットを再検出
            }

            if (target != null)
            {
                RotateTurret();
            }

            // 発射レートを超えたら弾を発射
            if (elapsedTime > currentRate && target != null)
            {
                Fire();
                elapsedTime = 0;
            }
        }


        /// <summary>
        /// タレットのレベルを上げる
        /// </summary>
        public void LevelUp()
        {
            currentLevel++;
            currentRate -= currentRate * rateMagnification; // レベルアップ時に発射間隔を短縮
        }

        /// <summary>
        /// 指定した範囲内のターゲットを検出
        /// </summary>
        private void DetectTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);
            if (colliders.Length > 0)
            {
                target = colliders[Random.Range(0, colliders.Length)].transform; // ランダムに選択
            }
        }


        /// <summary>
        /// ターゲットを狙って弾を発射
        /// </summary>
        private void Fire()
        {
            if (bulletPool == null)
                bulletPool = GameManager.I.GetEnemyPool();
            var bullet = bulletPool.GetObject(bulletPrefab); // プールから弾を取得
            if (bullet != null)
            {
                bullet.transform.position = shotPos.position; // タレットの位置から発射
                bullet.SetActive(true);
                bullet.transform.LookAt(target.position);
                // Rigidbody を取得してターゲットに向けて移動
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null && target != null)
                {
                    Vector3 direction = (target.position - bullet.transform.position).normalized;
                    rb.velocity = direction * bulletSpeed;
                }
            }
            fireCount++;

            // 5発ごとにターゲット変更
            if (fireCount >= changeTargetCount)
            {
                DetectTarget();
                fireCount = 0;  // カウントリセット
            }
        }

        public void Intaract()
        {
            if (currentLevel >= maxLevel)
            {
                pointText.text = "レベルが最大";
                return;
            }

            if (GameManager.I.GetPoint() < currentNeedPoint) return;
            LevelUp();
            GameManager.I.AddPoint(-currentNeedPoint);
            currentNeedPoint += (int)(currentNeedPoint * pointMagnification);
            pointText.text = "必要なポイント:" + currentNeedPoint.ToString();
        }

        /// <summary>
        /// タレットのターゲットを手動で設定
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// タレットの索敵範囲をギズモで可視化
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
        /// <summary>
        /// タレットをターゲットの方向にY軸のみ回転させる
        /// </summary>
        private void RotateTurret()
        {
            Vector3 direction = target.position - turret.position;
            direction.y = 0; // Y軸の変化を無視（XZ平面のみ考慮）

            if (direction != Vector3.zero) // 方向がゼロでない場合のみ回転
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                turret.rotation = targetRotation;
            }
        }
    }
}
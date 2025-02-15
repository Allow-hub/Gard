using System.Collections;
using TechC.Interface;
using TMPro;
using UnityEngine;

namespace TechC
{
    public enum AttackMode
    {
        Normal,
        Shooting
    }

    public class PlayerAttack : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private Swinging swinging;
        [SerializeField] private Rigidbody rb; // プレイヤーのRigidbody
        [SerializeField] private Animator anim;
        [SerializeField] private GameObject mainCam, attackCam;
        [SerializeField] private GameObject voiceObj;
        [SerializeField] private TextMeshProUGUI voiceText;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Setting")]
        [SerializeField] private float shotCooldown = 0.2f;
        [SerializeField] private float addPower = 20;
        [SerializeField] private float stopDuration = 1;
        [SerializeField] private float attackCooldown = 1.5f; // クールダウン時間
        [SerializeField] private float hitStop = 0.3f;
        [SerializeField] private float afterHit = 0.5f; //ヒット後力を半減
        [SerializeField] private Vector2 forceRange;
        [SerializeField] private float attackHeightOffset = 1.5f; // 少し上を狙う
        [SerializeField] private float bulletSpeed = 100;
        [SerializeField] private Transform shotPos;
        [SerializeField] private int bulletMp = 100;
        private bool canAttack = true; // クールダウン管理
        private Vector3 lastVelocity; // 最後の速度を記録する
        private const int extraDamage = 300; // 一撃用の特別ダメージ
        private bool playingVoice = false;

        private GameObject hitObj;
        public int damage = 10; // テスト用

        // 攻撃モード
        [SerializeField] private AttackMode currentAttackMode = AttackMode.Normal;

        private void Awake()
        {
            mainCam.SetActive(true);
            attackCam.SetActive(false);
        }

        private void Update()
        {
            // マウスホイールで攻撃モードを切り替える
            if (Input.mouseScrollDelta.y > 0)
            {
                currentAttackMode = AttackMode.Shooting;
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                currentAttackMode = AttackMode.Normal;
            }

            if (!canAttack) return; // クールダウン中は攻撃不可
            if (swinging.GetHitObject() == null) return;

            hitObj = swinging.GetHitObject();

            if (playerInputManager.IsAttacking && hitObj.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (currentAttackMode == AttackMode.Normal)
                    Attack();
                else
                    Shot();
            }
        }

        private void Shot()
        {
            if (!canAttack) return; // クールダウン中なら発射しない
            canAttack = false; // クールダウン開始

            if (objectPool == null)
                objectPool = GameManager.I.GetEnemyPool();
            StartCoroutine(Voice());

            // ターゲットが設定されていない場合は処理を中断
            if (hitObj == null) return;

            // オブジェクトプールから弾を取得
            var bullet = objectPool.GetObject(bulletPrefab);
            if (bullet != null)
            {
                // 発射位置を設定
                bullet.transform.position = shotPos.position;
                bullet.SetActive(true);

                // ターゲットの方向に弾の向きを調整
                bullet.transform.LookAt(hitObj.transform.position);

                // Rigidbody を取得してターゲットに向けて速度を設定
                Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
                if (rbBullet != null)
                {
                    Vector3 direction = (hitObj.transform.position - bullet.transform.position).normalized;
                    rbBullet.velocity = direction * bulletSpeed;
                }
            }
            GameManager.I.AddPlayerMp(-bulletMp);
            // クールダウン処理を開始
            StartCoroutine(ShotCooldown());
        }

        private IEnumerator ShotCooldown()
        {
            yield return new WaitForSeconds(shotCooldown); 
            canAttack = true; // 再び攻撃可能に
        }


        private void Attack()
        {
            if (!canAttack) return;
            canAttack = false; // 攻撃を実行したのでロック
            StartCoroutine(AttackAnim());
        }

        private IEnumerator Voice()
        {
            if (playingVoice) yield break;

            playingVoice = true;
            voiceObj.SetActive(true);
            // 攻撃モードに応じて異なる声を出す例
            if (currentAttackMode == AttackMode.Normal)
            {
                SeManager.I.PlaySE(0);

                voiceText.text = "いくよ！";
            }
            else if (currentAttackMode == AttackMode.Shooting)
            {
                SeManager.I.PlaySE(1);

                voiceText.text = "力を貸して";
            }
            yield return new WaitForSeconds(1f);
            playingVoice = false;
            voiceObj.SetActive(false);
        }

        private IEnumerator AttackAnim()
        {
            StartCoroutine(Voice());
            rb.useGravity = false; // 攻撃中は重力をオフ
            rb.velocity = Vector3.zero; // 速度をリセット

            mainCam.SetActive(false);
            attackCam.SetActive(true);
            playerController.StopPlayer(stopDuration);
            anim.SetBool("IsAttacking", true);

            yield return new WaitForSeconds(stopDuration);

            if (hitObj != null)
            {
                Vector3 targetPosition = hitObj.transform.position + Vector3.up * attackHeightOffset;
                Vector3 attackDirection = (targetPosition - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, targetPosition);
                float forceMagnitude = Mathf.Clamp(distance * addPower, forceRange.x, forceRange.y);

        

                // 目標到達までの時間を計算
                float estimatedTime = distance / forceMagnitude;

                // velocity で直接飛ばす
                rb.velocity = attackDirection * forceMagnitude;
                yield return new WaitForSeconds(estimatedTime);
            }

            anim.SetBool("IsAttacking", false);
            rb.useGravity = true; // 攻撃終了後に重力を戻す
            mainCam.SetActive(true);
            attackCam.SetActive(false);
            canAttack = true;
        }

        private IEnumerator Hit(Vector3 velocity, Vector3 dir)
        {
            playerController.StopPlayer(hitStop);
            yield return new WaitForSeconds(hitStop);
            rb.useGravity = true; // 攻撃終了後に重力を戻す
            rb.velocity = velocity * afterHit;
        }

        private void OnTriggerEnter(Collider collider)
        {
            // 衝突相手がIDamageableを実装しているか確認（親オブジェクトで判定）
            IDamageable damageable = collider.gameObject.transform.parent?.GetComponent<IDamageable>();

            if (collider.gameObject == hitObj)
            {
                Vector3 impactVelocity = rb.velocity;
                Vector3 impactDirection = impactVelocity.normalized;
                if (damageable != null)
                {
                    damageable.TakeDamage(extraDamage);
                    StartCoroutine(Hit(impactVelocity, impactDirection));

                }
            }
        }

        public AttackMode CurrentMode() => currentAttackMode;
        public bool GetAttacking() => canAttack;
    }
}

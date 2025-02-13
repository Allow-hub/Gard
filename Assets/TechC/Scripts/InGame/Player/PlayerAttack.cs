using System.Collections;
using TechC.Interface;
using UnityEngine;

namespace TechC
{
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

        [Header("Setting")]
        [SerializeField] private float addPower = 20;
        [SerializeField] private float stopDuration = 1;
        [SerializeField] private float attackCooldown = 1.5f; // クールダウン時間
        [SerializeField] private float hitStop = 0.3f;
        [SerializeField] private float afterHit = 0.5f; //ヒット後力を半減
        [SerializeField] private Vector2 forceRange;
        [SerializeField] private float attackHeightOffset = 1.5f; // 少し上を狙う

        private bool canAttack = true; // クールダウン管理
        private Vector3 lastVelocity; // 最後の速度を記録する
        private const int extraDamage = 1000;//一撃用の特別ダメージ

        private GameObject hitObj;
        public int damage = 10; //テスト

        private void Awake()
        {
            mainCam.SetActive(true);
            attackCam.SetActive(false);
        }


        private void Update()
        {
            if (!canAttack) return; // クールダウン中は攻撃不可
            if (swinging.GetHitObject() == null) return;

            hitObj = swinging.GetHitObject();

            if (playerInputManager.IsAttacking && hitObj.layer == LayerMask.NameToLayer("Enemy"))
            {
                Attack();
            }
        }

        private void Attack()
        {
            if (!canAttack) return;

            canAttack = false; // 攻撃を実行したのでロック
            StartCoroutine(AttackAnim());
        }

        private IEnumerator AttackAnim()
        {
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

                // 物理計算ではなく velocity で直接飛ばす
                rb.velocity = attackDirection * forceMagnitude;

                yield return new WaitForSeconds(estimatedTime);
            }

            anim.SetBool("IsAttacking", false);
            mainCam.SetActive(true);
            attackCam.SetActive(false);
            canAttack = true;
        }




        private IEnumerator Hit(Vector3 velocity,Vector3 dir)
        {
            playerController.StopPlayer(hitStop);
            yield return new WaitForSeconds(hitStop);
            rb.useGravity = true; // 攻撃終了後に重力を戻す

            rb.velocity = velocity*afterHit ;

        }

        private void OnTriggerEnter(Collider collider)
        {
            // 衝突相手が IDamageable を実装しているか確認
            IDamageable damageable = collider.gameObject.transform.parent?.GetComponent<IDamageable>();

            if (collider.gameObject == hitObj)
            {
                // 到達時の速度を記録
                Vector3 impactVelocity = rb.velocity;
                Vector3 impactDirection = impactVelocity.normalized;
                if (damageable != null)
                {
                    // ダメージを与える
                    damageable.TakeDamage(extraDamage);
                }
                //Debug.Log($"到達時の速度: {impactVelocity.magnitude}");
                //Debug.Log($"到達時の方向: {impactDirection}");
                StartCoroutine(Hit(impactVelocity,impactDirection));
            }
        }
        public bool GetAttacking() => canAttack;
    }
}

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


        [Header("Setting")]
        [SerializeField] private float stopDuration = 1;
        [SerializeField] private float attackCooldown = 1.5f; // クールダウン時間
        [SerializeField] private float hitStop = 0.3f;
        [SerializeField] private float afterHit = 0.5f; //ヒット後力を半減
        [SerializeField] private Vector2 forceRange;
        private bool canAttack = true; // クールダウン管理
        private Vector3 lastVelocity; // 最後の速度を記録する

        private GameObject hitObj;
        public int damage = 10; //テスト

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
            playerController.StopPlayer(stopDuration);
            // 攻撃開始前の待機
            yield return new WaitForSeconds(stopDuration );

            if (hitObj != null)
            {
                Vector3 attackDirection = (hitObj.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, hitObj.transform.position);
                float forceMagnitude = Mathf.Clamp(distance * 10f, forceRange   .x, forceRange.y);

                playerController.PlayerAddForce(attackDirection, forceMagnitude, ForceMode.Impulse);
            }


            // クールダウンを適用
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }

        private IEnumerator Hit(Vector3 velocity,Vector3 dir)
        {
            playerController.StopPlayer(hitStop);
            yield return new WaitForSeconds(hitStop);
            rb.velocity = velocity*afterHit ;

        }

        private void OnTriggerEnter(Collider collider)
        {
            // 衝突相手が IDamageable を実装しているか確認
            IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // ダメージを与える
                damageable.TakeDamage(damage);
            }
            if (collider.gameObject == hitObj)
            {
                // 到達時の速度を記録
                Vector3 impactVelocity = rb.velocity;
                Vector3 impactDirection = impactVelocity.normalized;

                //Debug.Log($"到達時の速度: {impactVelocity.magnitude}");
                //Debug.Log($"到達時の方向: {impactDirection}");
                StartCoroutine(Hit(impactVelocity,impactDirection));
            }
        }
    }
}

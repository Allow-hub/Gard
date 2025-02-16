using System.Collections;
using System.Collections.Generic;
using TechC.Interface;
using TechC.Status;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class Boss : MonoBehaviour, IDamageable
    {
        [Header("Reference")]
        [SerializeField] private Animator anim;
        [SerializeField] private EnemyStatus enemyStatus;
        [SerializeField] private GameObject bossObj;
        [SerializeField] private GameObject explosion;
        [SerializeField] private GameObject clearCanvas;
        private ObjectPool enemyPool;
        private ObjectPool soundPool;

        [Header("Animator")]
        private int hashIsAttacking1 = Animator.StringToHash("IsAttacking1");
        private int hashIsAttacking2 = Animator.StringToHash("IsAttacking2");
        private int hashIsDead = Animator.StringToHash("IsDead");
        private int hashIsRaging = Animator.StringToHash("IsRaging");
        private int hashIsDamaging = Animator.StringToHash("IsDamaging");

        [Header("Parameter")]
        [SerializeField] private string enemyName;
        [SerializeField] private Vector2 attackIntervalRange;
        [SerializeField] private int addPoint = 1000;
        [SerializeField] private TextMeshProUGUI hpTex;

        [SerializeField] private GameObject areaAttackBall;
        [SerializeField] private Vector3 areaAttackSize;
        [SerializeField] private float areaAttackDuration;
        [SerializeField] private GameObject areaEffect;

        private float elapsedTime = 0;
        private float currentInterval;
        private int currentHealth;
        private int lastHealth;

        private bool isAttacking = false; // 攻撃中フラグ


        private void OnEnable()
        {
            currentHealth = enemyStatus.maxHealth;
            lastHealth = currentHealth;
            hpTex.text = currentHealth.ToString();
            anim.SetBool(hashIsRaging, true);
            this.DelayMethod(3f, () => anim.SetBool(hashIsRaging, false));

            currentInterval = Random.Range(attackIntervalRange.x, attackIntervalRange.y);
        }

        private void Update()
        {
            if (lastHealth != currentHealth)
            {
                hpTex.text = currentHealth.ToString();
                lastHealth = currentHealth;
            }

            if (isAttacking) return; // 攻撃中なら何もしない

            elapsedTime += Time.deltaTime;
            if (elapsedTime > currentInterval)
            {
                var rand = Random.Range(0, 100);
                if (rand <= 50)
                {
                    ChasePlayerAttack();
                }
                else
                {
                    AreaAttack();
                }
                currentInterval = Random.Range(attackIntervalRange.x, attackIntervalRange.y);
                elapsedTime = 0;
            }
        }

        private void ChasePlayerAttack()
        {
            if (isAttacking) return; // 攻撃中なら中断

            isAttacking = true;
            //anim.SetTrigger(hashIsAttacking1);

            this.DelayMethod(1.5f, () => isAttacking = false); // 攻撃アニメーションの時間に応じて解除
        }

        private void AreaAttack()
        {
            if (isAttacking) return; // 攻撃中なら中断
            Debug.Log("Atta");
            isAttacking = true;
            StartCoroutine(AreaAttackCol());
        }

        private IEnumerator AreaAttackCol()
        {
            anim.SetBool(hashIsAttacking2, true);
            areaAttackBall.SetActive(true);
            areaAttackBall.transform.localScale = Vector3.zero;
            areaAttackBall.transform.position = bossObj.transform.position;

            float elapsed = 0f;
            while (elapsed < areaAttackDuration)
            {
                float progress = elapsed / areaAttackDuration;
                areaAttackBall.transform.localScale = Vector3.Lerp(Vector3.zero, areaAttackSize, progress);
                elapsed += Time.deltaTime;
                yield return null;
            }
            areaAttackBall.transform.localScale = areaAttackSize;

        
            areaEffect.SetActive(true);

            yield return new WaitForSeconds(0.5f);
            areaAttackBall.SetActive(false);
            anim.SetBool(hashIsAttacking2, false);
            areaEffect.SetActive(false);

            isAttacking = false; // 攻撃終了後にフラグ解除
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            anim.SetBool(hashIsDamaging, true);
            anim.SetBool(hashIsDamaging, false);
            if (currentHealth <= 0)
            {
                Death();
            }
        }

        public void Death()
        {
            GameManager.I.AddPoint(addPoint);
            anim.SetTrigger(hashIsDead);
            if (enemyPool == null || soundPool == null)
            {
                enemyPool = GameManager.I.GetEnemyPool();
                soundPool = GameManager.I.GetSoundPool();
            }
            this.DelayMethod(4f, () =>
            {
                var obj = enemyPool.GetObject(explosion);
                obj.transform.position = gameObject.transform.position;
                GameManager.I.ChangeClearState();
                clearCanvas.SetActive (true);
                gameObject.SetActive(false);
            });
        }
    }
}

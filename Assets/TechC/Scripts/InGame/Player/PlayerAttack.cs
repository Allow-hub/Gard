using TechC.Interface;
using UnityEngine;

namespace TechC
{
    public class PlayerAttack : MonoBehaviour
    {
        public int damage = 10; //テスト

        private void OnCollisionEnter(Collision collision)
        {
            // 衝突相手が IDamageable を実装しているか確認
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // ダメージを与える
                damageable.TakeDamage(damage);
            }
        }
    }
}

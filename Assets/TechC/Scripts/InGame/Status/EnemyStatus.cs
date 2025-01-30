using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Status
{
    [CreateAssetMenu(fileName = "EnemyStatus", menuName = "Game Data/Enemy Status")]
    public class EnemyStatus : ScriptableObject
    {
        [Header("基本ステータス")]
        public string enemyName;
        public int maxHealth;
        public int attackPower;
        public float moveSpeed;

    }
}

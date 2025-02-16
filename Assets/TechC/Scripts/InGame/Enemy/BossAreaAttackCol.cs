using System.Collections;
using System.Collections.Generic;
using TechC.Status;
using UnityEngine;

namespace TechC
{
    public class BossAreaAttackCol : MonoBehaviour
    {
        [SerializeField] private EnemyStatus enemyStatus;

        private bool isIn;

        private void OnEnable()
        {
            isIn = false;
        }

        private void OnDisable()
        {
            if (isIn)
            {
                Debug.Log("A");
                GameManager.I.AddPlayerHp(-enemyStatus.attackPower);

            }
            Debug.Log("Dis");

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("A");

                isIn = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                isIn = false;
            }
        }
    }
}

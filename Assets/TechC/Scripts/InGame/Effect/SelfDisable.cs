using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SelfDisable : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool isEnemy = false;
        private  ObjectPool objectPool; 
        void Start()
        {
            if(isEnemy) 
                objectPool = GameManager.I.GetEnemyPool();
            else
                objectPool =GameManager.I.GetSoundPool();
        }

        private void OnEnable()
        {
            StartCoroutine(Delay());
        }

        
        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(duration);
            objectPool.ReturnObject(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SelfDisable : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;
        private  ObjectPool objectPool; 
        void Start()
        {
            objectPool = transform.parent?.parent?.GetComponent<ObjectPool>();
        }

        private void OnEnable()
        {
            StopCoroutine(Delay());
            StartCoroutine(Delay());
        }

        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(duration);
            objectPool.ReturnObject(gameObject);
        }
    }
}

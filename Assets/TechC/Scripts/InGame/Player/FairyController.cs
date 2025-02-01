using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class FairyController : MonoBehaviour
    {
        [SerializeField] private Transform fairyPos;
        [SerializeField] private Transform playerRoatate;
        [SerializeField] private float moveSpeed = 1f;
   

        private void Update()
        {
            // フェアリーの位置がターゲット位置に向かって動く
            transform.position = Vector3.Lerp(transform.position, fairyPos.position, Time.deltaTime * moveSpeed);

           
        }

        private void LateUpdate()
        {
            transform.rotation = playerRoatate.rotation;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class RotAround : MonoBehaviour
    {
        [SerializeField] private Transform target; // 回転の中心となるターゲット
        [SerializeField] private float speed = 10f; 
        [SerializeField] private Vector3 axis = Vector3.up;

        void Update()
        {
            if (target != null)
            {
                // ターゲットの周囲を指定した軸で回転
                transform.RotateAround(target.position, axis, speed * Time.deltaTime);
            }
        }
    }
}

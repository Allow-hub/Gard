using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class CameraFrontUi : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        private void OnValidate()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        private void Update()
        {
            gameObject.transform.LookAt(player.transform.position);
        }
    }
}

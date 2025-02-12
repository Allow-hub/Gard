using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class TutCol : MonoBehaviour
    {
        [SerializeField] private GameObject nexObj;
        [SerializeField] private bool isLast;
        private bool once = false;
        private void OnTriggerEnter(Collider other)
        {
            if (once) return;
            if (nexObj != null)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
                    playerController.StopPlayer(1f);
                    nexObj.SetActive(true);
                }
            }

            if(isLast)
            {
                GameManager.I.ChangeInGameState();
            }
            once = true;
        }
    }
}

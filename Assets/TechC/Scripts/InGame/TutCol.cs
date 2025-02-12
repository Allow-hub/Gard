using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class TutCol : MonoBehaviour
    {
        [SerializeField] private GameObject nexObj;
        [SerializeField] private bool isLast;
        [SerializeField] private TutorialManager tutorialManager;
        private bool once = false;

        private void OnValidate()
        {
            tutorialManager = FindAnyObjectByType<TutorialManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (once) return;
            if (isLast)
            {
                tutorialManager.ChangeInGame();
                once =true; 
                return;
            }
            if (nexObj != null)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
                    playerController.StopPlayer(1f);
                    nexObj.SetActive(true);
                }
            }

        }
    }
}

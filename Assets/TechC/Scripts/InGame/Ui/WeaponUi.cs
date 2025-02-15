using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class WeaponUi : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private Sprite sword,gun;

        private AttackMode lastMode;

        private void Update()
        {
            if (playerAttack.CurrentMode() == lastMode) return;
            if(playerAttack.CurrentMode() == AttackMode.Normal)
                image.sprite = sword;
            else
                image.sprite = gun;
            lastMode = playerAttack.CurrentMode();
        }
    }
}

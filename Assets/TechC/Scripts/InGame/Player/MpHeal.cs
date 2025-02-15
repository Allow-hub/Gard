using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class MpHeal : MonoBehaviour
    {
        [SerializeField] private float interval = 1;
        [SerializeField] private int healAmount = 5;
        private float elapsedTime = 0;

        // Update is called once per frame
        void Update()
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime > interval)
            {
                GameManager.I.AddPlayerMp(healAmount);
                elapsedTime = 0;
            }
        
        }
    }
}

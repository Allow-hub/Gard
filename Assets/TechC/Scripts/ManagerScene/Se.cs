using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class Se : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();  
        }
        private void OnEnable()
        {
            if (SeManager.I == null) return;
            if (audioSource.volume == SeManager.I.GetSEVolume()) return;
            audioSource.volume = SeManager.I.GetSEVolume();
        }
    }
}

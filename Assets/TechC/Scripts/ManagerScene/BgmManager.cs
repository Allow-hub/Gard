using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class BgmManager : Singleton<BgmManager>
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip titleBgm, tutBgm, InGameBgm;

        protected override void Init()
        {
            base.Init();
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void SetTitleBgm()
        {
            audioSource.clip = titleBgm;
        }
        public void SetTutBgm()
        {
            audioSource.clip = tutBgm;
        }
        public void SetInGameBgm()
        {
            audioSource.clip = InGameBgm;
        }
        public void PlayBGM() => audioSource.Play();
        private void StopBGM() => audioSource.Stop();

        public void SetBGMVolume(float volume)
        {
            audioSource.volume = volume;
        }
    }
}

using UnityEngine;

namespace TechC
{
    public class SliderController : MonoBehaviour
    {
        public void SetBGMVolume(float volume)
        {
            BgmManager.I.SetBGMVolume(volume);
        }
        public void SetSEVolume(float volume)
        {
            SeManager.I.SetSEVolume(volume);
        }
        public void SetSensitivity(float volume)
        {
            GameManager.I.sensitivity = volume;
        }
    }
}

using Coffee.UIEffects;
using System.Collections;
using UnityEngine;
using TechC;

namespace TechC
{
    public class FadeManager : MonoBehaviour
    {
        [SerializeField] private UIEffect uIEffect;
        private float maxCount = 0.99f;
        private float minCount = 0;

        private void Awake()
        {
            if (uIEffect != null)
            {
                uIEffect.transitionRate = maxCount; // 最初は透明にする
            }
        }

        public void ShotFade(float duration)
        {
            StartFadeOut(duration / 2);
            this.DelayMethod(duration / 2, () => StartFadeIn(duration / 2));
        }


        public void StartFadeIn(float duration)
        {
            if (uIEffect != null)
            {
                StartCoroutine(FadeIn(duration));
            }
        }

        private IEnumerator FadeIn(float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);
                uIEffect.transitionRate = Mathf.Lerp(minCount, maxCount, progress);
                yield return null;
            }

            uIEffect.transitionRate = maxCount; // 確実にフェードイン完了
        }

        public void StartFadeOut(float duration)
        {
            if (uIEffect != null)
            {
                StartCoroutine(FadeOut(duration));
            }
        }

        private IEnumerator FadeOut(float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);
                uIEffect.transitionRate = Mathf.Lerp(maxCount, minCount, progress);
                yield return null;
            }

            uIEffect.transitionRate = minCount; // 確実にフェードアウト完了
        }
    }
}

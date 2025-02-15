using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TechC;

namespace TechC
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private float awakeFadeDuration = 3;
        [SerializeField] private Button tutButton, startButton;
        [SerializeField] private Vector2 voiceIntervalRange = new Vector2(10, 30);

        private float elapsedTime = 0;
        private float currentInterval;
        private FadeManager fadeManager;
        private void Start()
        {
            this.DelayMethod(1f, () => SeManager.I.PlaySE(7));
            currentInterval = Random.Range(voiceIntervalRange.x, voiceIntervalRange.y);
            GameManager.I.ChangeTitleState();
            fadeManager = GameManager.I.GetFadeManager();
            fadeManager.StartFadeIn(awakeFadeDuration / 2);
            tutButton.onClick.AddListener(() => TutStart());
            startButton.onClick.AddListener(() => StartButton());
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > currentInterval)
            {
                float rand = Random.Range(0, 100);
                if (rand >= 50)
                    SeManager.I.PlaySE(6);
                else
                    SeManager.I.PlaySE(7);
                currentInterval = Random.Range(voiceIntervalRange.x, voiceIntervalRange.y);

                elapsedTime = 0;
            }
        }
        public void TutStart()
        {
            this.DelayMethod(awakeFadeDuration / 2, () => GameManager.I.LoadSceneAsync(1));


            GameManager.I.Fade(awakeFadeDuration);
            this.DelayMethod(awakeFadeDuration / 2, () => GameManager.I.ChangeTutorialState());
        }

        public void StartButton()
        {
            this.DelayMethod(awakeFadeDuration / 2, () => GameManager.I.LoadSceneAsync(1));
            GameManager.I.Fade(awakeFadeDuration);
            this.DelayMethod(awakeFadeDuration / 2, () => GameManager.I.ChangeInGameState());

        }
    }
}

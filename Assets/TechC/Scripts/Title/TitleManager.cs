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
        private FadeManager fadeManager;
        private void Start()
        {
            GameManager.I.ChangeTitleState();
            fadeManager = GameManager.I.GetFadeManager();
            fadeManager.StartFadeIn(awakeFadeDuration / 2);
            tutButton.onClick.AddListener(() => TutStart());
            startButton.onClick.AddListener(() => StartButton());
        }


        public void TutStart()
        {
            this.DelayMethod(awakeFadeDuration/2,()=> GameManager.I.LoadSceneAsync(1));

            
            GameManager.I.Fade(awakeFadeDuration);
            this.DelayMethod(awakeFadeDuration/2,()=> GameManager.I.ChangeTutorialState());
        }

        public void StartButton()
        {
            GameManager.I.LoadSceneAsync(1);
            GameManager.I.Fade(awakeFadeDuration);
            this.DelayMethod(awakeFadeDuration/2, () => GameManager.I.ChangeInGameState());

        }
    }
}

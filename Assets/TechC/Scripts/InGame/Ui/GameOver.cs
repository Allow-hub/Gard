using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class GameOver : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverObj;
        [SerializeField] private Button titleButton;

        private void Start()
        {
            titleButton.onClick.AddListener(Title);
        }


        private void Title()
        {
            GameManager.I.LoadSceneAsync(0);
        }
        // Update is called once per frame
        void Update()
        {
            if(GameManager.I.currentState == GameManager.GameState.GameOver)
            {
                gameOverObj.SetActive(true);
            }

        }
    }
}

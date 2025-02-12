using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TechC
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private int targetFrameRate = 144;
        [SerializeField] private FadeManager fadeManager;
        public float sensitivity = 2;
        private bool CanPlay = true;
        private ObjectPool enemyAndEffectPool;
        private ObjectPool soundPool;
        public enum GameState
        {
            Title,
            Menu,
            Tutorial,
            InGame,
            Clear,
            GameOver
        }
        public GameState currentState = GameState.Title;
        protected override void Init()
        {
            base.Init();

            // VSyncCount を Dont Sync に変更
            QualitySettings.vSyncCount = 0;
            // fps 144 を目標に設定
            Application.targetFrameRate = targetFrameRate;
            SetState(GameState.Title);
        }


        private void Update()
        {
            StateHandler();
        }

        public void SetState(GameState state)
        {
            currentState = state;
            switch (state)
            {
                case GameState.Title:
                    TitleInit();
                    break;
                case GameState.Menu:
                    break;
                case GameState.Clear:
                    break;
                case GameState.GameOver:
                    break;
                case GameState.Tutorial:
                    TutorialInit();
                    break;
                case GameState.InGame:
                    InGameInit();
                    break;
            }
        }
        private void StateHandler()
        {
            switch (currentState)
            {
                case GameState.Title:
                    break;
                case GameState.Menu:
                    break;
                case GameState.Clear:
                    break;
                case GameState.GameOver:
                    break;
            }
        }

        private void ChangeCursorMode(bool visible, CursorLockMode cursorLockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = cursorLockMode;
        }


        // 非同期でシーンをロード
        public void LoadSceneAsync(int sceneIndex)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }

        // 非同期でシーンをロードするコルーチン
        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            asyncOperation.allowSceneActivation = false;

            // シーンのロードが終わるまで待機
            while (!asyncOperation.isDone)
            {
                // ロードが進んだら進行状況を表示
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                Debug.Log("Loading progress: " + (progress * 100) + "%");

                // ロードが完了したらシーンをアクティブ化
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        public FadeManager GetFadeManager() => fadeManager;
        public void Fade(float duration) => fadeManager.ShotFade(duration);

        public bool GetCanPlay() => CanPlay;
        public bool ChangeCanPlay() => CanPlay = !CanPlay;

        private void TitleInit() => ChangeCursorMode(transform, CursorLockMode.None);
        private void TutorialInit()
        {
            ChangeCursorMode(false, CursorLockMode.Locked);
            if (soundPool == null || enemyAndEffectPool == null)
            {
                soundPool = GameObject.Find("SoundPool").GetComponent<ObjectPool>();
                enemyAndEffectPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            }
        }
        private void InGameInit()
        {
            ChangeCursorMode(false, CursorLockMode.Locked);
            if (soundPool == null || enemyAndEffectPool == null)
            {
                Debug.Log("A");
                soundPool = GameObject.Find("SoundPool").GetComponent<ObjectPool>();
                enemyAndEffectPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            }
        }
        public ObjectPool GetSoundPool() => soundPool;
        public ObjectPool GetEnemyPool() => enemyAndEffectPool;

        public void ChangeTitleState() => SetState(GameState.Title);
        public void ChangeTutorialState() => SetState(GameState.Tutorial);
        public void ChangeInGameState() => SetState(GameState.InGame);
    }

}

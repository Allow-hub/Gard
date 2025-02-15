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
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private int bossDesCount = 30;
        public float sensitivity = 2;
        public int maxHp = 1000;
        public int maxMp = 1000;
        public int maxHomeHp = 1000;

        private bool CanPlay = true;
        private ObjectPool enemyAndEffectPool;
        private ObjectPool soundPool;
        private int point; //タレット強化用ポイント
        private int playerHp = 1000;
        private int playerMp = 1000;
        private int homeHp = 1000;
        private Transform player;
        private int DesCount = 0;
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
            //Shader.WarmupAllShaders();
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
                    GameOverInit();
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
                case GameState.Tutorial:
                    break;
                case GameState.InGame:
                    InGame();
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

        public int GetPlayerHp() => playerHp;
        public int GetPlayerMp() => playerMp;
        public void AddPlayerHp(int value)
        {
            if (value < 0)
            {
                SeManager.I.PlaySE(2);
            }

            playerHp += value;
            if (playerHp <= 0)
            {
                playerHp = 0;
            }
            else if (playerHp >= maxHp)
            {
                playerHp = maxHp;
            }
        }
        public void AddPlayerMp(int value)
        {
            playerMp += value;
            if (playerMp <= 0)
            {
                playerMp = 0;
            }
            else if (playerMp >= maxMp)
            {
                playerMp = maxMp;
            }
        }

        public int GetHomeHp() => homeHp;
        public void AddHomeHp(int value)
        {
            homeHp += value;
            if (homeHp <= 0)
            {
                homeHp = 0;
            }
            else if (homeHp >= maxHp)
            {
                homeHp = maxHp;
            }
        }
        public bool BossComing() => DesCount >= bossDesCount;
        public void AddDesCount() => DesCount++;

        //HPが0以下で死亡でfalse
        public bool IsAliving() => playerHp > 0 && homeHp > 0;
        public int GetPoint() => point;
        public void AddPoint(int value) => point += value;
        public FadeManager GetFadeManager() => fadeManager;
        public void Fade(float duration) => fadeManager.ShotFade(duration);

        public bool GetCanPlay() => CanPlay;
        public bool ChangeCanPlay() => CanPlay = !CanPlay;

        private void TitleInit()
        {
            DesCount = 0;
            point = 0;
            playerHp = maxHp;
            playerMp = maxMp;
            homeHp = maxHp;

            ChangeCursorMode(true, CursorLockMode.None);
            BgmManager.I.SetTitleBgm();
            BgmManager.I.PlayBGM();
        }
        private void TutorialInit()
        {
            ChangeCursorMode(false, CursorLockMode.Locked);
            if (soundPool == null || enemyAndEffectPool == null)
            {
                soundPool = GameObject.Find("SoundPool").GetComponent<ObjectPool>();
                enemyAndEffectPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            }
            BgmManager.I.SetTutBgm();
            BgmManager.I.PlayBGM();
        }
        private void InGameInit()
        {
            ChangeCursorMode(false, CursorLockMode.Locked);
            DesCount = 0;
            point = 0;
            playerHp = maxHp;
            playerMp = maxMp;
            homeHp = maxHp;

            AddPoint(200);
            BgmManager.I.SetInGameBgm();
            BgmManager.I.PlayBGM();
        }

        private void GameOverInit()
        {
            ChangeCursorMode(true, CursorLockMode.None);
            SeManager.I.PlaySE(5);
        }

        private void InGame()
        {
            if (!IsAliving())
                ChangeGameOverState();
        }

        public void SetMenu(bool val)
        {
            CanPlay = !val;
            menuCanvas.SetActive(val);
            Time.timeScale = val ? 0 : 1;
            ChangeCursorMode(val, val ? CursorLockMode.None : CursorLockMode.Locked);
        }
        public Transform GetPlayerTransform()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player").transform;
            return player.transform;
        }

        public float CalcDistanceFromPlayer(Vector3 pos)
        {
            if (player == null)
                player = GameObject.FindWithTag("Player").transform;
            return Vector3.Distance(pos, player.transform.position);
        }

        public ObjectPool GetSoundPool()
        {
            if (soundPool == null)
                soundPool = GameObject.Find("SoundPool").GetComponent<ObjectPool>();
            return soundPool;
        }
        public ObjectPool GetEnemyPool()
        {
            if (enemyAndEffectPool == null)
                enemyAndEffectPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            return enemyAndEffectPool;
        }

        public void ChangeTitleState() => SetState(GameState.Title);
        public void ChangeTutorialState() => SetState(GameState.Tutorial);
        public void ChangeInGameState() => SetState(GameState.InGame);
        public void ChangeGameOverState() => SetState(GameState.GameOver);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechC;

namespace TechC
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject player;
        [SerializeField] private Transform tutInitPos, inGameInitPos;
        [SerializeField] private GameObject[] explainObj;
        [SerializeField] private Transform[] enemyPos;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform minY;
        [SerializeField] private Transform warpPos;
        [SerializeField] private GameObject orderCanvas;
        [SerializeField] private float warpDuration = 3f;
        private bool isWarping = false;

        private void Start()
        {
            orderCanvas.SetActive(false);
            for (int i = 0; i < explainObj.Length; i++)
            {
                explainObj[i].gameObject.SetActive(false);
            }
            StartCoroutine(LateStart());
        }

        private void Update()
        {
            if (GameManager.I.currentState == GameManager.GameState.Tutorial&&!isWarping)
            {
                if (player.transform.position.y <= minY.position.y)
                {
                    player.transform.position = tutInitPos.position;
                }
            }
        }

        private IEnumerator LateStart()
        {
            yield return null; // Start の処理がすべて完了した後、次のフレームで実行
            if (GameManager.I.currentState == GameManager.GameState.Tutorial)
            {
                player.transform.position = tutInitPos.position;
                explainObj[0].SetActive(true);
                var enemy_1 = objectPool.GetObject(enemyPrefab);
                enemy_1.transform.position = enemyPos[0].position;
                var enemy_2 = objectPool.GetObject(enemyPrefab);
                enemy_2.transform.position = enemyPos[1].position;
                var enemy_3 = objectPool.GetObject(enemyPrefab);
                enemy_3.transform.position = enemyPos[2].position;
            }
            else
            {
                orderCanvas.SetActive(true);
                player.transform.position = inGameInitPos.position;
                gameObject.SetActive(false);
            }
        }

        public void ChangeInGame()
        {
            isWarping = true;
            player.transform.position = warpPos.position;

            // ワープ完了後、初期位置に戻す
            this.DelayMethod(warpDuration, () =>
            {
                orderCanvas.SetActive(true);
                SeManager.I.PlaySE(3);
                player.transform.position = inGameInitPos.position;
                GameManager.I.ChangeInGameState();
            });

            float delay = 0.1f;

            playerController.StopPlayer(warpDuration);

            // 非アクティブ化を一番最後に実行
            this.DelayMethod(warpDuration + delay, () =>
            {
                gameObject.SetActive(false);
            });
        }

    }
}

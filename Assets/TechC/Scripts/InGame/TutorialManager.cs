using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject player;
        [SerializeField] private Transform tutInitPos, inGameInitPos;
        [SerializeField] private GameObject[] explainObj;
        [SerializeField] private Transform[] enemyPos;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform minY;

        private void Start()
        {
            for (int i = 0; i < explainObj.Length; i++)
            {
                explainObj[i].gameObject.SetActive(false);
            }
            StartCoroutine(LateStart());
        }

        private void Update()
        {
            if (GameManager.I.currentState == GameManager.GameState.Tutorial)
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
                player.transform.position = inGameInitPos.position;
                gameObject.SetActive(false);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class ObjectPool : MonoBehaviour
    {
        [Header("Object Pool Settings")]
        [SerializeField] private ObjectPoolItem[] poolItems; // 各プレハブの設定をひとまとめにした配列

        // 各プレハブごとのオブジェクトプール
        private Dictionary<GameObject, Queue<GameObject>> objectPools = new Dictionary<GameObject, Queue<GameObject>>();

        // 初期化メソッド：コンストラクタの代わりにこのメソッドを使用
        public void Initialize(ObjectPoolItem[] items)
        {
            poolItems = items;
        }

        private void Awake()
        {
            if (poolItems == null || poolItems.Length == 0)
            {
                Debug.LogError("Object Poolの初期化が不足しています。Initializeメソッドを正しく呼び出してください。");
                return;
            }

            foreach (var poolItem in poolItems)
            {
                if (!objectPools.ContainsKey(poolItem.prefab))
                {
                    objectPools[poolItem.prefab] = new Queue<GameObject>();
                }

                for (int i = 0; i < poolItem.initialSize; i++)
                {
                    GameObject newObject = Instantiate(poolItem.prefab);
                    newObject.SetActive(false);
                    newObject.transform.SetParent(poolItem.parent.transform);
                    objectPools[poolItem.prefab].Enqueue(newObject);
                }
            }
        }


        // プレハブからオブジェクトを取得する
        public GameObject GetObject(GameObject prefab)
        {
            // プールにそのプレハブのオブジェクトがあれば、それを返す
            if (objectPools.ContainsKey(prefab) && objectPools[prefab].Count > 0)
            {
                GameObject pooledObject = objectPools[prefab].Dequeue();
                pooledObject.SetActive(true);
                return pooledObject;
            }
            else
            {
                // プールに無ければ新しく作成
                GameObject newObject = Instantiate(prefab);
                return newObject;
            }
        }

        // オブジェクトをプールに返却する
        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false); // オブジェクトを非アクティブにする
            GameObject prefab = GetPrefabFromObject(obj);

            if (prefab != null && objectPools.ContainsKey(prefab))
            {
                obj.transform.SetParent(GetParentFromPrefab(prefab).transform); // プールの親オブジェクトに設定
                objectPools[prefab].Enqueue(obj); // プールに戻す
            }
            else
            {
                Debug.LogWarning($"オブジェクトの返却が失敗しました: {obj.name}。プールに紐づいていない可能性があります。");
                Destroy(obj); // プレハブが見つからなければオブジェクトを破棄
            }
        }


        // オブジェクトからそのプレハブを取得する
        private GameObject GetPrefabFromObject(GameObject obj)
        {
            foreach (var kvp in objectPools)
            {
                if (obj.name.Contains(kvp.Key.name)) // 名前が一致する場合に対応
                {
                    return kvp.Key;
                }
            }
            Debug.LogWarning($"対応するプレハブが見つかりませんでした: {obj.name}");
            return null;
        }


        // プレハブから親オブジェクトを取得する
        private GameObject GetParentFromPrefab(GameObject prefab)
        {
            foreach (var poolItem in poolItems)
            {
                if (poolItem.prefab == prefab)
                {
                    return poolItem.parent;
                }
            }
            return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [System.Serializable]
    public class ObjectPoolItem
    {
        public string name;
        public GameObject prefab;      // プールするプレハブ
        public GameObject parent;      // プールの親オブジェクト
        public int initialSize;        // 初期サイズ
    }

}

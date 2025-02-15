using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class ChangeHdr : MonoBehaviour
    {
        [Header("スカイボックスの設定")]
        [SerializeField] private Material skybox1; // スカイボックス1
        [SerializeField] private Material skybox2; // スカイボックス2

        private Button button;
        private bool isUsingSkybox1 = true;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => Change());
        }

        /// <summary>
        /// スカイボックスを切り替えるメソッド（ボタンのOnClickに登録）
        /// </summary>
        public void Change()
        {
            if (isUsingSkybox1)
            {
                RenderSettings.skybox = skybox2;
            }
            else
            {
                RenderSettings.skybox = skybox1;
            }
            isUsingSkybox1 = !isUsingSkybox1;

            // 環境照明の更新（必要に応じて）
            DynamicGI.UpdateEnvironment();
        }
    }
}

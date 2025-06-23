using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class ClearUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI pointTex, desCountTex;
        [SerializeField] private Button button;
        private void Start()
        {
            button.onClick.AddListener(() => GameManager.I.LoadSceneAsync(0));
        }
        private void OnEnable()
        {
            pointTex.text = "強化ポイント:" + GameManager.I.GetPoint();
            desCountTex.text = "討伐数:" + GameManager.I.GetDesCount();

        }
    }
}

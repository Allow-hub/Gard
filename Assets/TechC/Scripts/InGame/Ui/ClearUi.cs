using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class ClearUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI pointTex, desCountTex;

        private void OnEnable()
        {
            pointTex.text = "強化ポイント:" + GameManager.I.GetPoint();
            desCountTex.text = "討伐数:" + GameManager.I.GetDesCount();

        }
    }
}

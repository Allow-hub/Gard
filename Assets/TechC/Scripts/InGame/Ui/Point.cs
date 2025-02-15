using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class Point : MonoBehaviour
    {
        private TextMeshProUGUI tex;
        private int lastPoint;

        private void Start()
        {
            tex= GetComponent<TextMeshProUGUI>();
            UpdatePoint();
        }
        private void Update()
        {
            UpdatePoint();
        }


        private void UpdatePoint()
        {
            if (GameManager.I.GetPoint() == lastPoint) return;
            tex.text = "強化ポイント:" + GameManager.I.GetPoint();
            lastPoint = GameManager.I.GetPoint();
        }
    }
}

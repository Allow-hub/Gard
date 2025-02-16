using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class Point : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI desText;
        private TextMeshProUGUI tex;
        private int lastPoint;
        private int lastDesCount;

        private void Start()
        {
            tex= GetComponent<TextMeshProUGUI>();
            UpdatePoint();
        }
        private void Update()
        {
            UpdatePoint();
            UpdateDesCount();
        }


        private void UpdatePoint()
        {
            if (GameManager.I.GetPoint() == lastPoint) return;
            tex.text = "強化ポイント:" + GameManager.I.GetPoint();
            lastPoint = GameManager.I.GetPoint();
        }
        private void UpdateDesCount()
        {
            if (GameManager.I.GetDesCount() == lastDesCount) return;
            desText.text = "討伐数:" + GameManager.I.GetDesCount();
            lastDesCount = GameManager.I.GetDesCount();
        }
    }
}

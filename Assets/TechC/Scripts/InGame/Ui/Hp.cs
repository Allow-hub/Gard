using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class Hp : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI homeHpText;
        [SerializeField] private Image homeHpFill;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI mpText;
        [SerializeField] private Image mpFill;

        private int lastHp;
        private int lastMp;
        private int lastHomeHp;

        private void Start()
        {
            // HPの初期表示

            hpText.text = "HP:" + GameManager.I.GetPlayerHp();
            hpFill.fillAmount = (float)GameManager.I.GetPlayerHp() / GameManager.I.maxHp;
            lastHp = GameManager.I.GetPlayerHp();


            // MPの初期表示
            mpText.text = "MP:" + GameManager.I.GetPlayerMp();
            mpFill.fillAmount = (float)GameManager.I.GetPlayerMp() / GameManager.I.maxMp;
            lastMp = GameManager.I.GetPlayerMp();

            // 拠点HPの初期表示
            homeHpText.text = "拠点のHP:" + GameManager.I.GetHomeHp();
            homeHpFill.fillAmount = (float)GameManager.I.GetHomeHp() / GameManager.I.GetHomeHp();
            lastHomeHp = GameManager.I.GetHomeHp();
        }

        private void Update()
        {
            if (GameManager.I.GetPlayerHp() != lastHp)
            {
                hpText.text = "HP:" + GameManager.I.GetPlayerHp();
                hpFill.fillAmount = (float)GameManager.I.GetPlayerHp() / GameManager.I.maxHp;
                lastHp = GameManager.I.GetPlayerHp();
            }

            if (GameManager.I.GetPlayerMp() != lastMp)
            {
                mpText.text = "MP:" + GameManager.I.GetPlayerMp();
                mpFill.fillAmount = (float)GameManager.I.GetPlayerMp() / GameManager.I.maxMp;
                lastMp = GameManager.I.GetPlayerMp();
            }
            if (GameManager.I.GetHomeHp() != lastHomeHp)
            {
                homeHpText.text = "拠点のHP:" + GameManager.I.GetHomeHp();
                homeHpFill.fillAmount = (float)GameManager.I.GetHomeHp() / GameManager.I.GetHomeHp();
                lastHomeHp = GameManager.I.GetHomeHp();
            }
        }
    }
}

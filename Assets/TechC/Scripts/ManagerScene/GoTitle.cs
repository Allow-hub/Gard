using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class GoTitle : MonoBehaviour
    {

        private Button button;
        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => Title());
        }

        private void Title()
        {
            GameManager.I.SetMenu(false);
            GameManager.I.LoadSceneAsync(0);
        }
    }
}

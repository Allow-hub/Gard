using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechC;
namespace TechC
{
    public class Order : MonoBehaviour
    {

        private void OnEnable()
        {
            this.DelayMethod(1f, () => SeManager.I.PlaySE(3));

            this.DelayMethod(5f,()=>gameObject.SetActive(false));
        }
    }
}

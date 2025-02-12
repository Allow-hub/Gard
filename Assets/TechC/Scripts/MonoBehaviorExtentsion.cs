using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace TechC
{

    /// <summary>
    /// MonoBehaviorの拡張クラス
    /// </summary>
    public static class MonoBehaviourExtension
    {
        public static void DelayMethod<T1, T2>(this MonoBehaviour mono, float waitTime, Action<T1, T2> action, T1 t1, T2 t2)
        {
            mono.StartCoroutine(DelayCoroutine(waitTime, () => action(t1, t2)));
        }

        public static void DelayMethod<T>(this MonoBehaviour mono, float waitTime, Action<T> action, T t)
        {
            mono.StartCoroutine(DelayCoroutine(waitTime, () => action(t)));
        }

        public static void DelayMethod(this MonoBehaviour mono, float waitTime, Action action)
        {
            mono.StartCoroutine(DelayCoroutine(waitTime, action));
        }

        private static IEnumerator DelayCoroutine(float waitTime, Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action?.Invoke();
        }
    }

}

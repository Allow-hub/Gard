using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class Reticle : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private Image reticle;
        [SerializeField] private UIEffect uiEffect;
        private RectTransform reticleTrans;

        [Header("Setting")]
        [SerializeField] private float distance = 1f;
        [SerializeField] private float sphereRadius = 3f;
        [SerializeField] private LayerMask focusLayer;
        [SerializeField] private Color focusColor,focusColor_Effect;
        private Color initColor, initColor_Effect;


        private Transform cam;
        [SerializeField] private Canvas canvas;

        [Header("Idle Movement Settings")]
        [SerializeField] private float idleSpeed = 1.5f;        // 動きの基本速度
        [SerializeField] private float idleAmplitude = 40f;     // 8の字の基本的な大きさ
        [SerializeField] private float noiseIntensity = 20f;    // ノイズの揺れ具合

        [Header("Scaling Settings")]
        [SerializeField] private float scaleSpeed = 2f;         // 拡大・縮小の速さ
        [SerializeField] private float minScale = 0.8f;         // 最小スケール
        [SerializeField] private float maxScale = 1.2f;         // 最大スケール

        private Vector2 idleCenter;  // 8の字の中心位置
        private float idleTime;      // 時間経過の追跡

        private void Awake()
        {
            cam = Camera.main.transform;
            reticleTrans = reticle.GetComponent<RectTransform>();

            initColor = reticle.color;
            initColor_Effect = uiEffect.edgeColor;

            idleCenter = Vector2.zero; // 画面中央を中心
        }

        private void Update()
        {
            Focus();
            AnimateScale();
        }

        private void Focus()
        {
            RaycastHit sphereCastHit;
            Vector3 sphereCastOrigin = cam.position;
            Vector3 sphereCastDirection = cam.forward;

            Physics.SphereCast(sphereCastOrigin, sphereRadius, sphereCastDirection, out sphereCastHit, distance, focusLayer);
            RaycastHit raycastHit;
            Physics.Raycast(sphereCastOrigin, sphereCastDirection, out raycastHit, distance, focusLayer);

            Vector3 realHitPoint;

            if (raycastHit.point != Vector3.zero)
                realHitPoint = raycastHit.point;
            else if (sphereCastHit.point != Vector3.zero)
                realHitPoint = sphereCastHit.point;
            else
                realHitPoint = Vector3.zero;

            if (realHitPoint != Vector3.zero)
            {
                if (reticle.color != focusColor)
                    reticle.color = focusColor;
                if (uiEffect.edgeColor != focusColor)
                    uiEffect.edgeColor = focusColor_Effect;
                if (Camera.main == null) return;
                // ヒット時はリティクルを固定
                Vector3 screenPos = Camera.main.WorldToScreenPoint(realHitPoint);
                Vector2 localPoint;
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, out localPoint))
                {
                    reticleTrans.anchoredPosition = localPoint;
                }
            }
            else
            {
                if (reticle.color != initColor)
                    reticle.color = initColor;
                if (uiEffect.edgeColor != initColor_Effect)
                    uiEffect.edgeColor = initColor_Effect;
                // 不規則な8の字の動き
                idleTime += Time.deltaTime * idleSpeed;

                // 基本の8の字パターン（異なる周波数で不規則感を出す）
                float x = Mathf.Sin(idleTime * 1.3f) * idleAmplitude;
                float y = Mathf.Sin(idleTime * 2.1f + Mathf.PI / 4) * (idleAmplitude * 0.6f);

                // ノイズによるランダムな揺れを追加
                float noiseX = (Mathf.PerlinNoise(idleTime * 0.5f, 0f) - 0.5f) * noiseIntensity;
                float noiseY = (Mathf.PerlinNoise(0f, idleTime * 0.8f) - 0.5f) * noiseIntensity;

                // 8の字の動きとノイズを組み合わせる
                Vector2 irregularMovement = new Vector2(x + noiseX, y + noiseY);

                reticleTrans.anchoredPosition = idleCenter + irregularMovement;
            }
        }

        private void AnimateScale()
        {
            // サイン波で自然な拡大縮小を表現
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * scaleSpeed) + 1f) / 2f);
            reticleTrans.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

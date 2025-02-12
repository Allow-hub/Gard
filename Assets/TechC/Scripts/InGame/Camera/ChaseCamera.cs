using System.Collections;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class ChaseCamera : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform player;
        [SerializeField] private float distance = 5.0f;
        [SerializeField] private float height = 2.0f;
        [SerializeField] private float shakeDuration = 0.5f; // シェイクの時間
        [SerializeField] private float shakeMagnitude = 0.3f; // シェイクの強度
        [SerializeField] private float dampingSpeed = 1.0f; // 減衰スピード
        [SerializeField] private Vector3 additionalOffset = new Vector3(0.5f, 0.2f, 0);
        [SerializeField] private GameObject speedEffect;

        [SerializeField] private float rbSpeed = 70;
        private float initDistance;
        private Camera cam;
        private float rotationX = 0.0f;
        private float rotationY = 0.0f;
        private const float minYAngle = -90.0f;
        private const float maxYAngle = 90.0f;
        private Vector3 shakeOffset = Vector3.zero;
        private float initialShakeMagnitude;
        private float initFov;

        [Header("WallCheck")]
        // 現在の位置
        private Vector3 targetPosition;

        // 目的地
        private Vector3 desiredPosition;

        // 壁の衝突情報
        private RaycastHit wallHit;

        // 壁に当たった位置
        private Vector3 wallHitPosition;

        // 衝突する壁のレイヤー
        [SerializeField] private LayerMask wallLayers;

        private Coroutine distanceChangeCoroutine;
        private Coroutine shakeCoroutine;
        private Coroutine fovChangeCoroutine;
        private void Start()
        {
            SetSpeedEffect(false);
            cam = Camera.main;
            initFov = cam.fieldOfView;
            //player = FindPlayerTransform();
            initialShakeMagnitude = shakeMagnitude;
            initDistance = distance;    
        }


        
        private void Update()
        {
            if (player == null)
            {
                player = FindPlayerTransform();
                if (player == null) return;
            }

            // マウスの動きを取得
            float mouseX = Input.GetAxis("Mouse X") * GameManager.I.sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * GameManager.I.sensitivity;

            // 上下回転を更新
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

            // 左右回転を更新
            rotationY += mouseX;

            // プレイヤーの位置を基にカメラの位置を計算
            Vector3 baseOffset = new Vector3(0, height, -distance);  // 元のカメラ位置のオフセット
            Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

            // 右上にカメラを配置するためのオフセット
            Vector3 offset = baseOffset + additionalOffset;  // オフセットに調整を加える

            targetPosition = player.position;  // プレイヤーの現在位置
            desiredPosition = targetPosition + rotation * offset; // 調整後の目的地を計算

            // 壁チェック
            if (WallCheck())
            {
                // 壁に衝突している場合、カメラ位置を調整
                desiredPosition = wallHitPosition + (desiredPosition - targetPosition).normalized * 0.5f;
            }

            // カメラの位置を更新
            cam.transform.position = desiredPosition + shakeOffset;

            // カメラの回転を更新
            cam.transform.LookAt(player.position + Vector3.up * height);

            if(rb.velocity.magnitude >= rbSpeed)
            {
                if (!speedEffect.activeSelf)
                    speedEffect.SetActive(true);
            }
            else
            {
                if (speedEffect.activeSelf)
                    speedEffect.SetActive(false);
            }
        }


        public void UpCamera(float duration, float newDistance)
        {
            if (distanceChangeCoroutine != null)
            {
                StopCoroutine(distanceChangeCoroutine);
            }
            distanceChangeCoroutine = StartCoroutine(ChangeDistanceOverTime(duration, newDistance));
        }

        private IEnumerator ChangeDistanceOverTime(float duration, float newDistance)
        {

            float elapsedTime = 0f;
            float initialDistance = distance;

            while (elapsedTime < duration)
            {
                distance = Mathf.Lerp(initialDistance, newDistance, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                //Debug.Log($"Distance updating: {distance}");

                yield return null;
            }

            distance = newDistance; // 最終的に直接設定
        }

        public float GetInitFov() => initFov;

        // FOVを滑らかに変化させるメソッド
        public void ChangeFOV(float targetFOV, float duration)
        {
            if (fovChangeCoroutine != null)
            {
                StopCoroutine(fovChangeCoroutine);  // 前のコルーチンが実行中なら停止
            }
            fovChangeCoroutine = StartCoroutine(ChangeFOVCoroutine(targetFOV, duration));
        }

        // FOV変更のコルーチン
        private IEnumerator ChangeFOVCoroutine(float targetFOV, float duration)
        {
            float startFOV = cam.fieldOfView;  // 現在のFOV
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // 線形補間でFOVを変更
                cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 最後に目標のFOVに設定
            cam.fieldOfView = targetFOV;
        }
    
    public void TriggerShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(Shake());
        }

        private IEnumerator Shake()
        {
            float elapsed = 0.0f;
            shakeMagnitude = initialShakeMagnitude; // シェイク強度を初期化

            while (elapsed < shakeDuration)
            {
                // 徐々にシェイクの強度を減衰させる
                float damper = 1.0f - (elapsed / shakeDuration);

                // Perlinノイズを使ったスムーズな揺れ
                float shakeX = (Mathf.PerlinNoise(Time.time * dampingSpeed, 0) - 0.5f) * 2 * shakeMagnitude * damper;
                float shakeY = (Mathf.PerlinNoise(0, Time.time * dampingSpeed) - 0.5f) * 2 * shakeMagnitude * damper;
                shakeOffset = new Vector3(shakeX, shakeY, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            shakeOffset = Vector3.zero; // シェイク終了時にオフセットをリセット
        }
        private bool WallCheck()
        {
            if (Physics.Raycast(targetPosition, desiredPosition - targetPosition, out wallHit, Vector3.Distance(targetPosition, desiredPosition), wallLayers, QueryTriggerInteraction.Ignore))
            {
                wallHitPosition = wallHit.point; // 壁に衝突した位置を保存
                return true; // 壁に衝突した
            }
            else
            {
                return false; // 壁に衝突しなかった
            }
        }

        public void SetSpeedEffect(bool value)=>speedEffect.gameObject.SetActive(value);

        private Transform FindPlayerTransform()
        {
            return GameObject.FindWithTag("Player")?.transform;
        }

        public float GetInitDistance()=>initDistance;   
    }
}

using System.Collections;
using UnityEngine;

namespace TechC
{
    public class Swinging : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] protected PlayerController playerController;
        [SerializeField] private LineRenderer lr;
        [SerializeField] private Transform gunTip, cam, player;
        [SerializeField] private LayerMask whatIsGrappleable;
        [SerializeField] private Transform orientation;
        [SerializeField] private Animator anim;

        private const string animName = "IsGrappling";
        [Header("Joint")]
        [SerializeField] private float spring = 4.5f;
        [SerializeField] private float damper = 7f;
        [SerializeField] private float mathScale = 10f;

        [Header("Prediction")]
        [SerializeField] private RaycastHit predictionHit;
        [SerializeField] private float predictionShereCastRadius;
        [SerializeField] private Transform predictionPoint;

        [Header("OdmGear")]
        [SerializeField] private float horizontalThrustForce;
        [SerializeField] private float forwardThurstForce;
        [SerializeField] private float extendCableSpeed;

        [Header("Swinging")]
        [SerializeField] float maxSwingDistance;
        [SerializeField] float startWidth;
        [SerializeField] float endWidth;
        [SerializeField] private float distanceFromPointMin = 0.5f;
        [SerializeField] private float distanceFromPointMax = 0.1f;

        [Header("Support")]

        [SerializeField] private float stopSuportTime = 0.3f;
        [SerializeField] private float addHight = 3f;
        [SerializeField] private Vector2 forceRange;
        // クールダウン用のフラグを追加
        private bool supportOnCooldown = false;

        [SerializeField] private bool isDrawingGizmo = false;
        private Vector3 swingPoint;
        private SpringJoint joint;
        private bool isSwinging = false;

        Rigidbody rb;
        private Vector3 currentGrapplePosition;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            lr.startWidth = startWidth;
            lr.endWidth = endWidth;
        }

        private void Update()
        {
            CheckForSwingPoint();
        }

        private void LateUpdate()
        {
            DrawRope();
        }

        void DrawRope()
        {
            if (!joint) return;
            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8);
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, swingPoint);
        }

        public void StartSwing()
        {
            if (predictionHit.point == Vector3.zero) return;

            // Swing対象が特定のタグを持っているかチェック
            if (predictionHit.collider != null && predictionHit.collider.CompareTag("Support"))
            {
                // 特定のタグを持っている場合の別処理を実行
                Debug.Log("特別なスイング対象がヒットしました！");
                StartSupport();
                return;
            }

            isSwinging = true;
            StartCoroutine(Stopping());
            anim.SetBool(animName, true);
            swingPoint = predictionHit.point;

            // SpringJointの作成
            if (joint == null)
            {
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = swingPoint;
                joint.spring = spring;
                joint.damper = damper;
                joint.massScale = mathScale;
                lr.positionCount = 2;
            }

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
            joint.maxDistance = distanceFromPoint * distanceFromPointMax;
            joint.minDistance = distanceFromPoint * distanceFromPointMin;
            currentGrapplePosition = gunTip.position;
        }

        private IEnumerator Stopping()
        {
            yield return new WaitForSeconds(0.3f);
            Vector3 directionToSwingPoint = (swingPoint - player.position).normalized;
            playerController.PlayerAddForce(directionToSwingPoint, forwardThurstForce, ForceMode.Impulse);
        }

        public void StopSwing()
        {
            anim.SetBool(animName, false);
            isSwinging = false;
            lr.positionCount = 0;
            Destroy(joint);
        }

        // Support処理を行うメソッド（クールダウン付き）
        private void StartSupport()
        {
            if (supportOnCooldown) return;
            supportOnCooldown = true;
            StopCoroutine(Support());
            StartCoroutine(Support());
        }

        private IEnumerator Support()
        {
            // プレイヤーを一定時間停止させる
            playerController.StopPlayer(stopSuportTime);
            playerController.ResetDownwardForce();

            swingPoint = predictionHit.point;

            // SpringJointの作成（既に存在しなければ）
            if (joint == null)
            {
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = swingPoint;
                joint.spring = spring;
                joint.damper = damper;
                joint.massScale = mathScale;
                lr.positionCount = 2;
            }

            // 現在の距離を使ってJointの距離設定（既存処理）
            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
            joint.maxDistance = distanceFromPoint * distanceFromPointMax;
            joint.minDistance = distanceFromPoint * distanceFromPointMin;
            currentGrapplePosition = gunTip.position;

            yield return new WaitForSeconds(stopSuportTime);
            StopSwing();

            // 目標位置はヒット位置から少し上方向（addHight分オフセット）
            Vector3 targetPos = new Vector3(predictionHit.point.x, predictionHit.point.y + addHight, predictionHit.point.z);
            Vector3 direction = (targetPos - player.position).normalized;

            // 現在の距離（playerとswingPoint間）に基づいて、力を線形補間する
            // t = 0 のとき：distance が distanceFromPointMin 付近 → 力は forceRange.x
            // t = 1 のとき：distance が maxSwingDistance 付近 → 力は forceRange.y
            float currentDistance = Vector3.Distance(player.position, swingPoint);
            float t = Mathf.InverseLerp(distanceFromPointMin, maxSwingDistance, currentDistance);
            float computedForce = Mathf.Lerp(forceRange.x, forceRange.y, t);

            // 計算された力でImpulseを加える
            playerController.PlayerAddForce(direction, computedForce, ForceMode.Impulse);

            yield return new WaitForSeconds(1f);
            supportOnCooldown = false;
        }

        private void CheckForSwingPoint()
        {
            if (joint != null) return;

            RaycastHit sphereCastHit;
            Vector3 sphereCastOrigin = cam.position;
            Vector3 sphereCastDirection = cam.forward;

            Physics.SphereCast(sphereCastOrigin, predictionShereCastRadius, sphereCastDirection, out sphereCastHit, maxSwingDistance, whatIsGrappleable);

            RaycastHit raycastHit;
            Physics.Raycast(sphereCastOrigin, sphereCastDirection, out raycastHit, maxSwingDistance, whatIsGrappleable);

            Vector3 realHitPoint;
            if (raycastHit.point != Vector3.zero)
                realHitPoint = raycastHit.point;
            else if (sphereCastHit.point != Vector3.zero)
                realHitPoint = sphereCastHit.point;
            else
                realHitPoint = Vector3.zero;

            if (realHitPoint != Vector3.zero)
            {
                predictionPoint.gameObject.SetActive(true);
                predictionPoint.position = realHitPoint;
            }
            else
            {
                predictionPoint.gameObject.SetActive(false);
            }

            predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
        }

        void OnDrawGizmos()
        {
            if (!isDrawingGizmo) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(predictionHit.point, predictionShereCastRadius);
        }
    }
}

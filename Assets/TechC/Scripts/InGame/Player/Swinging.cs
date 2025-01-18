using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private bool isDrawingGizmo = false;
        private Vector3 swingPoint;
        private SpringJoint joint;
        private bool isSwinging = false;

        Rigidbody rb;
        private Vector3 currentGrapplePosition;


        // public bool canSwing;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();

            lr.startWidth = startWidth;
            lr.endWidth = endWidth;
        }

        private void Update()
        {
            CheckForSwingPoint();

            if (playerInputManager.IsSwinging)
            {
                StartSwing();
                isSwinging = true;

                //if (joint != null) OdmGearMovement();
            }
            else
            {
                StopSwing();

            }

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

                // ロープを描画
                lr.positionCount = 2;
            }

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // SpringJointの距離設定
            joint.maxDistance = distanceFromPoint * distanceFromPointMax;
            joint.minDistance = distanceFromPoint * distanceFromPointMin;
            currentGrapplePosition = gunTip.position;

         }

        private IEnumerator Stopping()
        {
            playerController.ChangeFreezingState();
            yield return new WaitForSeconds(0.3f);
            playerController.ChangeFreezingState();
            // スウィングポイントに向かって力を加える
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
        //private void OdmGearMovement()
        //{
        //    if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime * 2, ForceMode.Force);
        //    if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime * 2, ForceMode.Force);
        //    if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * forwardThurstForce * Time.deltaTime * 10, ForceMode.Force);

        //    if (Input.GetKey(KeyCode.Space))
        //    {
        //        Vector3 directionToPoint = swingPoint - transform.position;
        //        rb.AddForce(directionToPoint.normalized * forwardThurstForce * Time.deltaTime * 2);

        //        float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);
        //        joint.maxDistance = distanceFromPoint * 0.8f;
        //        joint.minDistance = distanceFromPoint * 0.25f;
        //    }
        //    if (Input.GetKey(KeyCode.S))
        //    {
        //        float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

        //        joint.maxDistance = extendedDistanceFromPoint * 0.8f;
        //        joint.minDistance = extendedDistanceFromPoint * 0.25f;
        //    }

        //}
        private void CheckForSwingPoint()
        {
            if (joint != null) return;

            RaycastHit sphereCastHit;
            Vector3 sphereCastOrigin = cam.position;
            Vector3 sphereCastDirection = cam.forward;

            // SphereCastの方向を可視化
            Debug.DrawRay(sphereCastOrigin, sphereCastDirection * maxSwingDistance, Color.red);  // 赤い線でキャスト方向を表示

            Physics.SphereCast(sphereCastOrigin, predictionShereCastRadius, sphereCastDirection, out sphereCastHit, maxSwingDistance, whatIsGrappleable);

            RaycastHit raycastHit;
            Physics.Raycast(sphereCastOrigin, sphereCastDirection, out raycastHit, maxSwingDistance, whatIsGrappleable);

            Vector3 realHitPoint;

            // ヒットした場所がある場合
            if (raycastHit.point != Vector3.zero)
                realHitPoint = raycastHit.point;
            // SphereCastが当たった場合
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
            Gizmos.color = Color.red; // 色の指定
            Gizmos.DrawWireSphere(predictionHit.point, predictionShereCastRadius); // 球を描画
        }


    }
}

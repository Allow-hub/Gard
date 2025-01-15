using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private Animator anim;
         private Rigidbody rb;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;            // 基本移動速度
        [SerializeField] private float rotationSpeed = 2f;        // 回転速度
        [SerializeField] private float decelerationFactor = 2f;   // 減速の強さ
        private const string walkAnimName = "IsWalking";
        private bool isFreezing = false;
        private Camera playerCamera;
        [Header("Dash")]
        [SerializeField] private float dashSpeedMultiplier = 2f;  // 通常速度の倍率

        private const string dashAnimName = "IsDashing";



        [Header("Jump")]
        [SerializeField] private float jumpForce = 3f;            // ジャンプ力
        [SerializeField] private float jumpCoolTime = 1f;         // ジャンプのクールタイム

        [SerializeField] private float forwardJumpForce = 15f; // 進行方向ジャンプ力（前方方向）
        [SerializeField] private float forwardJumpMultiplier = 1.5f; // 進行方向ジャンプ時の強さ調整

        [SerializeField] private float height = 2;
        [SerializeField] private LayerMask groundLayer;  // Ground用のレイヤーマスク
        [SerializeField] private float groundCheckDistance = 0.1f;  // レイキャストの距離

        private const string jumpAnimName = "IsJumping";
        private bool canJump = true;

        private void Awake()
        {
            playerCamera = Camera.main;
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Debug.Log(IsGrounded());
            if (!GameManager.I.GetCanPlay()||isFreezing)
            {
                rb.velocity = Vector3.zero;
                return;
            }

            if (canJump && playerInputManager.IsJumping)
                Jump();
        }

        private void FixedUpdate()
        {
            if (!GameManager.I.GetCanPlay() || isFreezing) return;
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector3 inputVector = playerInputManager.InputVector;
            if (inputVector == Vector3.zero)
            {
                anim.SetBool(walkAnimName, false);
                anim.SetBool(dashAnimName, false);
                return;
            }

            // カメラの向きに基づいて移動ベクトルを計算
            Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRight = new Vector3(playerCamera.transform.right.x, 0, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = (cameraForward * inputVector.z + cameraRight * inputVector.x).normalized;

            // ダッシュ速度を適用
            float currentSpeed = playerInputManager.IsDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
            Vector3 adjustedMovement = movementDirection * currentSpeed;

            // プレイヤーの移動
            rb.MovePosition(rb.position + adjustedMovement * Time.deltaTime);

            // プレイヤーの向きを移動方向に合わせる
            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // アニメーション設定
            anim.SetBool(walkAnimName, !playerInputManager.IsDashing);
            anim.SetBool(dashAnimName, playerInputManager.IsDashing);
        }



        private void Jump()
        {

            // カメラの向きをベースにしたジャンプ
            Vector3 cameraForward = playerCamera.transform.forward; // カメラの前方向ベクトル
            cameraForward.Normalize(); // 正規化してベクトルの長さを1にする

            // カメラの方向と上方向の力を組み合わせてジャンプ
            Vector3 jumpDirection = (cameraForward + Vector3.up).normalized;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldown());
            ////動いていないときのジャンプ
            //if (canJump && IsGrounded()&&playerInputManager.InputVector==Vector3.zero)
            //{
            //    // 垂直ジャンプ
            //    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //    canJump = false;
            //    StartCoroutine(JumpCooldown());
            //}
            ////動いているとき動いている方向に強めのジャンプ
            //else if (canJump &&IsGrounded() && playerInputManager.InputVector != Vector3.zero)
            //{
            //    // 上方向の力
            //    Vector3 upwardJumpDirection = Vector3.up;
            //    rb.AddForce(upwardJumpDirection * jumpForce, ForceMode.Impulse);

            //    // 進行方向の力
            //    Vector3 forwardJumpDirection = rb.velocity.normalized + Vector3.up * 2f;  // 上向きの力を少し加える
            //    rb.AddForce(forwardJumpDirection * forwardJumpForce * forwardJumpMultiplier, ForceMode.Impulse);

            //    canJump = false;
            //    StartCoroutine(JumpCooldown());
            //}
            ////空中でのジャンプキー
            //else if(canJump && !IsGrounded())
            //{
            //    // 上方向の力
            //    Vector3 upwardJumpDirection = Vector3.up;
            //    rb.AddForce(upwardJumpDirection * jumpForce, ForceMode.Impulse);

            //    // 進行方向の力
            //    Vector3 forwardJumpDirection = (rb.velocity.normalized + Vector3.up).normalized;
            //    rb.AddForce(forwardJumpDirection * forwardJumpForce * forwardJumpMultiplier, ForceMode.Impulse);

            //    canJump = false;
            //    StartCoroutine(JumpCooldown());
            //}


        }

        // ジャンプクールダウンのコルーチン
        private IEnumerator JumpCooldown()
        {
            anim.SetBool(jumpAnimName, true);
            yield return new WaitForSeconds(jumpCoolTime);
            anim.SetBool(jumpAnimName, false);

            canJump = true;

        }

        private bool IsGrounded()
        {
            // オブジェクトの下方向へレイキャスト
            Vector3 origin =new Vector3(transform.position.x,transform.position.y+height,transform.position.z);
            Vector3 direction = Vector3.down;
            float distance = groundCheckDistance;


            // レイキャストで接地判定
            if (Physics.Raycast(origin, direction, distance, groundLayer))
            {
                return true;  // GroundLayerに接触している
            }

            return false;  // 接触していない
        }
        private void OnDrawGizmos()
        {
            // レイキャストの開始位置と方向を設定
            Vector3 origin = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
            Vector3 direction = Vector3.down * groundCheckDistance;

            // Gizmoの色を設定
            Gizmos.color = Color.red;

            // 下方向へのレイキャストを描画
            Gizmos.DrawRay(origin, direction);

            // 球を使ったレイの終点の可視化
            //Gizmos.DrawWireSphere(origin + direction, 0.05f);
        }

        public void PlayerAddForce(Vector3 dir ,float force , ForceMode forceMode) =>rb.AddForce(dir*force,forceMode);
        public void ChangeFreezing()=>isFreezing =!isFreezing;
    }
}

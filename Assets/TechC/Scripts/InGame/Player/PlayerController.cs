using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private Rigidbody rb;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;            // 基本移動速度
        [SerializeField] private float rotationSpeed = 2f;        // 回転速度
        [SerializeField] private float limit = 1.2f;             // ボールの速度の何倍まで許容するか
        [SerializeField] private float decelerationFactor = 2f;   // 減速の強さ
        private const string walkAnimName = "IsWalking";

        private Camera playerCamera;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 3f;            // ジャンプ力
        [SerializeField] private float jumpCoolTime = 1f;         // ジャンプのクールタイム
        [SerializeField] private float teamJumpForce;

        private const string jumpAnimName = "IsJumping";
        private bool canJump = true;



        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            // 入力に基づく移動ベクトルを計算
            Vector3 inputVector = playerInputManager.InputVector;
            Vector3 movement = new Vector3(inputVector.x, 0f, inputVector.z).normalized * moveSpeed;

            if (inputVector == Vector3.zero)
            {
                // 止まっている場合、移動アニメーションを停止
                //anim.SetBool(walkAnimName, false);
                return;
            }

            // カメラの向きに基づいて移動ベクトルを調整
            Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRight = new Vector3(playerCamera.transform.right.x, 0, playerCamera.transform.right.z).normalized;
            Vector3 adjustedMovement = (cameraForward * inputVector.z + cameraRight * inputVector.x).normalized * moveSpeed;

            // プレイヤーの移動
            // 他の物理演算との干渉を防ぐために座標操作で移動させている
            rb.MovePosition(rb.position + adjustedMovement * Time.deltaTime);

            // プレイヤーを移動方向に向ける
            if (adjustedMovement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(adjustedMovement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // 移動中はアニメーションを発火
            //anim.SetBool(walkAnimName, true);
        }
    }
}

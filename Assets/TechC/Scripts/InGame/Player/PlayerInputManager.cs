using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC
{
    public class PlayerInputManager : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] private InputActionAsset inputActionAsset;

        public Vector3 InputVector => inputVector;
        public bool IsMoving => isMoving;
        public bool IsDashing => isDashing;
        public bool IsJumping => isJumping;
        public bool IsAttacking => isAttacking;  // 攻撃状態を管理する
        public bool IsSwinging => isSwinging;  // Swinging状態を管理する

        private Vector3 inputVector;
        private Vector3 moveInput;
        private bool isMoving = false;
        private bool isDashing = false;
        private bool isJumping = false;
        private bool isAttacking = false;
        private bool isSwinging = false;

        private float yMovement = 0f;
        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // XZ平面とY軸の入力ベクトルを更新
            inputVector = new Vector3(moveInput.x, yMovement, moveInput.y);
            isMoving = moveInput != Vector3.zero || yMovement != 0f;
        }

        // Moveアクションが実行されたときの処理 (XZ軸)
        public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

        public void OnDashing(InputAction.CallbackContext context)
        {
            if (context.performed) return;

            if (context.started)
            {
                isDashing = true;  // ボタンが押された瞬間
            }
            else if (context.canceled)
            {
                isDashing = false;  // ボタンが離された瞬間
            }
        }

        public void OnSwinging(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isSwinging = true;  // ボタンが押された瞬間
            }
            else if (context.canceled)
            {
                isSwinging = false;  // ボタンが離された瞬間
            }
        }

        // Jumpアクションが実行されたときの処理 (Y軸を+1にする)
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            isJumping = true;  // ジャンプ状態をtrueに
            StartCoroutine(Delay(0));
        }

        // Attackアクションが実行されたときの処理
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            isAttacking = true;  // 攻撃状態をtrueに
            StartCoroutine(Delay(1));
        }

        private IEnumerator Delay(int n)
        {
            yield return new WaitForSeconds(0.1f);
            if (n == 0)
            {
                ResetJumping();
            }
            if (n == 1)
            {
                ResetAttacking();
            }
            else if (n == 2)
            {
                ResetSwinging();
            }
        }

        public void OnMenu(InputAction.CallbackContext context)
        {
            //if (GameManager.I == null) return;
            //if (GameManager.I.currentState == GameManager.GameState.Menu)
            //    GameManager.I.ChangeLastState();
            //else
            //    GameManager.I.ChangeMenuState();
        }

        public void ResetJumping() => isJumping = false;
        public void ResetAttacking() => isAttacking = false;
        public void ResetSwinging() => isSwinging = false;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace TechC
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Jumping,
        Swinging,
        Freezing
    }

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private Swinging swinging;
        [SerializeField] private ChaseCamera chaseCamera;
        [SerializeField] private Animator anim;
        private Rigidbody rb;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float decelerationFactor = 2f;

        [SerializeField] private float walkCameraDuration = 0.5f;
        [SerializeField] private float walkFovDuration = 0.5f;

        private Camera playerCamera;

        [Header("Dash")]
        [SerializeField] private float dashSpeedMultiplier = 2f;
        [SerializeField] private float dashCameraDistance = 0.5f;
        [SerializeField] private float dashCameraDuration = 0.5f;
        [SerializeField] private float dashFovDuration = 0.5f;
        [SerializeField] private float dashFov = 0.5f;

        [Header("Jump")]
        [SerializeField] private float jumpStoppingTime = 0.3f;
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float jumpCoolTime = 1f;
        [SerializeField] private Transform orientation;
        [SerializeField] private GameObject smokeEffect;
        [SerializeField] private float height = 2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float jumpFov = 80;
        [SerializeField] private float jumpFovDuration = 1;

        [SerializeField] private PlayerState currentState = PlayerState.Idle;
        private PlayerState lastState = PlayerState.Moving;

        private bool canJump = true;
        private bool wasDashing;
        private void Awake()
        {
            playerCamera = Camera.main;
            rb = GetComponent<Rigidbody>();
            smokeEffect.SetActive(false);
        }

        private void Update()
        {
            if (!GameManager.I.GetCanPlay())
            {
                rb.velocity = Vector3.zero;
                return;
            }

            //Vector3 forward = new Vector3(playerCamera.transform.forward.x, playerCamera.transform.forward.x, playerCamera.transform.forward.z).normalized;
            //if (forward != Vector3.zero)
            //{
            //    orientation.rotation = Quaternion.LookRotation(forward);
            //}

            StateHandler();
        }

        private void FixedUpdate()
        {
            if (!GameManager.I.GetCanPlay() || currentState == PlayerState.Freezing) return;
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector3 inputVector = playerInputManager.InputVector;
            bool isMoving = inputVector != Vector3.zero;
            bool isDashing = playerInputManager.IsDashing;
            if (playerInputManager.IsSwinging||playerInputManager.IsJumping) return;

            if (isMoving)
            {
                ChangeState(PlayerState.Moving);
                MovePlayer(inputVector, isDashing);
            }
            else
            {
                ChangeIdleState();
            }
        }




        private void MovePlayer(Vector3 inputVector, bool currentIsDashing)
        {
            Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRight = new Vector3(playerCamera.transform.right.x, 0, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = (cameraForward * inputVector.z + cameraRight * inputVector.x).normalized;

            float currentSpeed = currentIsDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
            Vector3 adjustedMovement = movementDirection * currentSpeed;

            rb.MovePosition(rb.position + adjustedMovement * Time.deltaTime);

            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (currentIsDashing != wasDashing)
            {
                float targetDistance = currentIsDashing ? dashCameraDistance : chaseCamera.GetInitDistance();
                float targetDuration = currentIsDashing ? dashCameraDuration : walkCameraDuration;
                chaseCamera.UpCamera(targetDuration, targetDistance);
                float targetFov = currentIsDashing ? dashFov : chaseCamera.GetInitFov();
                float targetFovDuration = currentIsDashing ? dashFovDuration : walkFovDuration;
                chaseCamera.ChangeFOV(targetFov, targetFovDuration);

                chaseCamera.SetSpeedEffect(currentIsDashing);
                wasDashing = currentIsDashing;
            }
            // アニメーションの状態を毎フレーム更新
            if (playerInputManager.InputVector != Vector3.zero)
                anim.SetBool("IsWalking", !currentIsDashing);
            anim.SetBool("IsDashing", currentIsDashing);
        }

        //ステート中の更新処理
        private void UpdateState()
        {
            switch (currentState)
            {
                case PlayerState.Idle:
                    break;
                case PlayerState.Moving:

                    break;
                case PlayerState.Jumping:
                    Jump();
                    break;
                case PlayerState.Swinging:
                    //swinging.SwingMove();
                    break;
                case PlayerState.Freezing:
                    rb.velocity = Vector3.zero;
                    break;
            }
        }

        //ステートの切り替え条件
        private void StateHandler()
        {
            UpdateState();

            if (currentState == lastState) return;
            if (playerInputManager.IsSwinging)
            {
                ChangeSwingingState();
            }
            else
            {
                swinging.StopSwing();
            }

            if (playerInputManager.IsJumping)
                ChangeState(PlayerState.Jumping);

        }

        //ステートの切り替え
        private void ChangeState(PlayerState newState)
        {
            if (currentState == newState) return;
            lastState = currentState;
            currentState = newState;


            anim.SetBool("IsJumping", newState == PlayerState.Jumping);

            switch (newState)
            {
                case PlayerState.Idle:
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("IsDashing", false);

                    break;
                case PlayerState.Moving:
                    break;

                case PlayerState.Jumping:
                    break;
                case PlayerState.Swinging:
                    swinging.StartSwing();
                    break;
                case PlayerState.Freezing:
                    break;
            }
        }

        private void Jump()
        {

            if (!canJump /*|| !IsGrounded()*/) return;
            StartCoroutine(JumpCooldown());
        }
    

        private IEnumerator JumpCooldown()
        {
            //メリハリのための静止
            rb.velocity = Vector3.zero;
            yield return new WaitForSeconds(jumpStoppingTime);
            chaseCamera.ChangeFOV(jumpFov,jumpFovDuration);
            //ジャンプ
            Vector3 jumpDirection = (Camera.main.transform.forward + Vector3.up).normalized;

            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            smokeEffect.SetActive(true);
            yield return new WaitForSeconds(jumpCoolTime);
            chaseCamera.ChangeFOV(chaseCamera.GetInitFov(), jumpFovDuration);

            smokeEffect.SetActive(false);
            canJump = true;
        }

        private bool IsGrounded()
        {
            Vector3 origin = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
            return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
        }

        public void PlayerAddForce(Vector3 dir, float force, ForceMode forceMode) => rb.AddForce(dir * force, forceMode);
        public void ChangeIdleState() => ChangeState(PlayerState.Idle);
        public void ChangeMovingState() => ChangeState(PlayerState.Moving);
        public void ChangeSwingingState() => ChangeState(PlayerState.Swinging);
        public void ChangeFreezingState() => ChangeState(PlayerState.Freezing);
    }
}

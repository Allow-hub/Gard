using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Dashing,
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
        private Camera playerCamera;

        [Header("Dash")]
        [SerializeField] private float dashSpeedMultiplier = 2f;
        [SerializeField] private float dashCameraDistance = 0.5f;
        [SerializeField] private float dashCameraDuration = 0.5f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float jumpCoolTime = 1f;
        [SerializeField] private Transform orientation;
        [SerializeField] private GameObject smokeEffect;
        [SerializeField] private float height = 2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.1f;

        private PlayerState currentState = PlayerState.Idle;
        private bool canJump = true;

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

            Vector3 forward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
            if (forward != Vector3.zero)
            {
                orientation.rotation = Quaternion.LookRotation(forward);
            }
            if (playerInputManager.IsSwinging)
                ChangeSwingingState();

            if (playerInputManager.IsJumping)
                ChangeJumpingState();
            StateHandler();
        }

        private void FixedUpdate()
        {
            if (!GameManager.I.GetCanPlay() || currentState == PlayerState.Freezing) return;
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (currentState == PlayerState.Swinging) return;   
            Vector3 inputVector = playerInputManager.InputVector;
            bool isMoving = inputVector != Vector3.zero;
            bool isDashing = playerInputManager.IsDashing;

            if (isMoving)
            {
                ChangeState(isDashing ? PlayerState.Dashing : PlayerState.Walking);
                MovePlayer(inputVector, isDashing);
            }
            else
            {
                ChangeIdleState();
            }
        }

        private void MovePlayer(Vector3 inputVector, bool isDashing)
        {
            Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRight = new Vector3(playerCamera.transform.right.x, 0, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = (cameraForward * inputVector.z + cameraRight * inputVector.x).normalized;

            float currentSpeed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
            Vector3 adjustedMovement = movementDirection * currentSpeed;

            rb.MovePosition(rb.position + adjustedMovement * Time.deltaTime);

            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }



        private void StateHandler()
        {
            switch (currentState)
            {
                case PlayerState.Idle:
                    // Idle状態では移動しない。
                    break;

                case PlayerState.Walking:
                    // Walking状態では移動処理を行う
                    HandleMovement();
                    break;

                case PlayerState.Dashing:
                    // Dashing状態ではダッシュ中の移動処理を行う
                    HandleMovement();
                    break;

                case PlayerState.Jumping:
                        Jump();
                    break;

                case PlayerState.Swinging:
                    // Swinging状態のとき、移動制限やカメラの調整などを行う
                    break;

                case PlayerState.Freezing:
                    // Freezing状態では移動を止めるなど、制限を加える
                    rb.velocity = Vector3.zero; // プレイヤーを止める
                    break;
            }
        }


        private void ChangeState(PlayerState newState)
        {
            if (currentState == newState) return;
            currentState = newState;

            anim.SetBool("IsWalking", newState == PlayerState.Walking);
            anim.SetBool("IsDashing", newState == PlayerState.Dashing);
            anim.SetBool("IsJumping", newState == PlayerState.Jumping);

            switch (newState)
            {
                case PlayerState.Idle:
                    break;
                case PlayerState.Walking:
                    chaseCamera.UpCamera(walkCameraDuration, chaseCamera.GetInitDistance());
                    break;
                case PlayerState.Dashing:
                    chaseCamera.UpCamera(dashCameraDuration, dashCameraDistance);
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
            if (!canJump || !IsGrounded()) return;

            ChangeState(PlayerState.Jumping);
            Vector3 jumpDirection = (orientation.forward + Vector3.up).normalized;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldown());
        }

        private IEnumerator JumpCooldown()
        {
            smokeEffect.SetActive(true);
            yield return new WaitForSeconds(jumpCoolTime);
            ChangeState(PlayerState.Idle);
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

        public void ChangeWalkingState() => ChangeState(PlayerState.Walking);

        public void ChangeDashingState() => ChangeState(PlayerState.Dashing);

        public void ChangeJumpingState() => ChangeState(PlayerState.Jumping);

        public void ChangeSwingingState() => ChangeState(PlayerState.Swinging);

        public void ChangeFreezingState() => ChangeState(PlayerState.Freezing);

    }
}

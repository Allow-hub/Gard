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
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float walkCameraDuration = 0.5f;
        [SerializeField] private float walkFovDuration = 0.5f;

        //[SerializeField] private int changeGravity = 13;
        //[SerializeField] private float changeDuration = 2f; //重力を変える期間
        //[SerializeField] private float changeGravityTime = 1f; //　重力を変え始める時間

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

        private bool isIncreasingDownwardForce = false;
        [SerializeField] private float maxAdditionalDownwardForce = 20f; // 追加する下向きの力の最大値（例：20）
        private Coroutine downwardForceCoroutine = null;

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
            AnimationHandler();
            //Vector3 forward = new Vector3(playerCamera.transform.forward.x, playerCamera.transform.forward.x, playerCamera.transform.forward.z).normalized;
            //if (forward != Vector3.zero)
            //{
            //    orientation.rotation = Quaternion.LookRotation(forward);
            //}

            StateHandler();
            // 地上にいなければ、下向きの力を徐々に増加させる処理を開始する
            if (!IsGrounded() && !isIncreasingDownwardForce)
            {
                downwardForceCoroutine = StartCoroutine(ApplyIncreasingDownwardForce());
            }
        }

        private void FixedUpdate()
        {
            if (!GameManager.I.GetCanPlay() || currentState == PlayerState.Freezing) return;
            HandleMovement();
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

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
            //if (playerInputManager.InputVector != Vector3.zero)
            //    anim.SetBool("IsWalking", !currentIsDashing);
            //anim.SetBool("IsDashing", currentIsDashing);
        }


        private void AnimationHandler()
        {
            // ゲームがプレイ可能でない場合は処理を行わない
            if (!GameManager.I.GetCanPlay())
                return;

            bool grounded = IsGrounded();

            // 浮いている（地上にいない）場合は、IsFloating を true にして、他のアニメーション更新は行わない
            if (!grounded)
            {
                anim.SetBool("IsFloating", true);
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsDashing", false);

                return;
            }
            else
            {
                // 地上に着いたら、IsFloating を false にする
                anim.SetBool("IsFloating", false);

                // Swinging状態でなければ、すべての Bool パラメーターをリセット
                if (currentState != PlayerState.Swinging)
                {
                    foreach (AnimatorControllerParameter param in anim.parameters)
                    {
                        if (param.type == AnimatorControllerParameterType.Bool)
                        {
                            anim.SetBool(param.name, false);
                        }
                    }
                }
            }

            // 入力状態などに基づいたアニメーションの更新

            bool isMoving = playerInputManager.InputVector != Vector3.zero;
            bool isDashing = playerInputManager.IsDashing;
            bool isSwinging = playerInputManager.IsSwinging;
            bool isJumping = playerInputManager.IsJumping;

            // Swinging 中は他の状態に干渉しない
            if (isSwinging)
            {
                anim.SetBool("IsSwinging", true);
            }
            else if (isJumping)
            {
                anim.SetBool("IsJumping", true);
            }
            else if (isMoving)
            {
                if (!grounded) return;
                // 移動中の場合、ダッシュかウォークかで分岐
                if (isDashing)
                {
                    anim.SetBool("IsDashing", true);
                }
                else
                {
                    anim.SetBool("IsWalking", true);
                }
            }
            // 移動していない場合は、リセットされているので、Animator のデフォルトステート（Idle）が再生される
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
                    //anim.SetBool("IsWalking", false);
                    //anim.SetBool("IsDashing", false);

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
            ResetDownwardForce();

            StartCoroutine(JumpCooldown());
        }
    

        private IEnumerator JumpCooldown()
        {
            //メリハリのための静止
            StopPlayer(jumpStoppingTime);
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

        public void StopPlayer(float value)
        {
            StartCoroutine(StopDelay(value));
        }

        private IEnumerator StopDelay(float value)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitForSeconds(value);
            rb.constraints =RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

        }

        /// <summary>
        /// 空中にいる間、1.5秒の待機後、3秒かけて下向きの追加力を徐々に増加させるコルーチン
        /// </summary>
        private IEnumerator ApplyIncreasingDownwardForce()
        {
            isIncreasingDownwardForce = true;

            // 1.5秒待機（空中でしばらく浮いた後）
            yield return new WaitForSeconds(1.5f);

            float duration = 3f; // 追加力を増加させる期間
            float elapsed = 0f;

            // 3秒かけて力を徐々に増加させる
            while (elapsed < duration && !IsGrounded())
            {
                // 0から maxAdditionalDownwardForce まで線形補間
                float t = elapsed / duration;
                float currentForce = Mathf.Lerp(0f, maxAdditionalDownwardForce, t);

              
                rb.AddForce(Vector3.down * currentForce, ForceMode.Acceleration);

                elapsed += Time.deltaTime;
                yield return null;
            }

            isIncreasingDownwardForce = false;
        }
        /// <summary>
        /// 下向きの追加力のコルーチンをリセットするメソッド
        /// </summary>
        public void ResetDownwardForce()
        {
            if (downwardForceCoroutine != null)
            {
                StopCoroutine(downwardForceCoroutine);
                downwardForceCoroutine = null;
            }
            isIncreasingDownwardForce = false;
        }
        public void ChangeIdleState() => ChangeState(PlayerState.Idle);
        public void ChangeMovingState() => ChangeState(PlayerState.Moving);
        public void ChangeSwingingState() => ChangeState(PlayerState.Swinging);
        public void ChangeFreezingState() => ChangeState(PlayerState.Freezing);
    }
}

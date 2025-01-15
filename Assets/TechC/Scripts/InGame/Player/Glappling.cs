using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class Glappling : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private Transform gunTip;
        [SerializeField] private LayerMask whatIsGrappleable;
        [SerializeField] private LineRenderer lr;
        [SerializeField] private Animator anim;

        private const string grappleAnimName = "IsGrappling";
        private Transform cam;

        [Header("Grappling")]
        [SerializeField] private float maxDistance;
        [SerializeField] private float delay;
        private bool isFreezing = false;

        private Vector3 grapplePoint;

        [SerializeField] private float coolDown;
        private float coolDownTimer;
        private bool isGrappling;

        private void Awake()
        {
            cam = Camera.main.transform;
        }

        private void Update()
        {
            if (playerInputManager.IsGrappling)
                StartGrapple();

            if (coolDownTimer > 0)
                coolDownTimer -= Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (isGrappling)
                lr.SetPosition(0, gunTip.position);
        }

        private void StartGrapple()
        {
            if (coolDownTimer > 0) return;

            isGrappling = true;
            isFreezing =true;
            RaycastHit hit;
            if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, whatIsGrappleable))
            {
                grapplePoint = hit.point;
                Invoke(nameof(ExecuteGrapple), delay);
            }
            else
            {
                grapplePoint = cam.position + cam.forward * maxDistance;
                Invoke(nameof(StopGrapple), delay);
            }
            lr.enabled = true;
            lr.SetPosition(1, grapplePoint);
        }

        private void ExecuteGrapple()
        {
            isFreezing = false;
        }

        private void StopGrapple()
        {
            isGrappling = false;
            isFreezing=false;
            coolDownTimer = coolDown;
            lr.enabled = false;
        }

        public bool GetFreeing() => isFreezing;
    }
}

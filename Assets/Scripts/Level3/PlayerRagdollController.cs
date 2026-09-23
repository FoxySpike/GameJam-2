using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerRagdollController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerMovementV3 movement;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerLookV2 playerLook;

    [Header("Ragdoll configurado con Unity Ragdoll Wizard")]
    [SerializeField] private Rigidbody hipsBody;

    [Header("Impacto y camara")]
    [SerializeField, Min(0f)] private float upwardVelocity = 2f;
    [SerializeField] private Transform cameraRig;
    [SerializeField, Min(0.1f)] private float cameraFollowSpeed = 8f;

    private bool movementWasEnabled;
    private bool animationControllerWasEnabled;
    private bool lookWasEnabled;
    private Vector3 recoveryForward;

    private Transform cameraOriginalParent;
    private Vector3 cameraOriginalLocalPosition;
    private Quaternion cameraOriginalLocalRotation;
    private Vector3 cameraOffsetFromHips;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    public bool IsRagdollActive { get; private set; }
    public bool IsReady => hipsBody != null && ragdollBodies != null && ragdollBodies.Length > 0;

    private void Awake()
    {
        FindPlayerReferences();
        FindRagdollParts();
        SetRagdollPhysics(false);

        if (!IsReady)
            Debug.LogWarning("Configure The Boss ragdoll with Unity Ragdoll Wizard. The simple hit reaction will be used meanwhile.", this);
    }

    private void LateUpdate()
    {
        if (!IsRagdollActive || cameraRig == null || hipsBody == null) return;

        Vector3 targetPosition = hipsBody.position + cameraOffsetFromHips;
        float blend = 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime);
        cameraRig.position = Vector3.Lerp(cameraRig.position, targetPosition, blend);
    }

    public bool BeginRagdoll(Vector3 impactDirection, float impactForce)
    {
        if (!IsReady || IsRagdollActive) return false;

        IsRagdollActive = true;
        RememberPlayerState();
        PrepareCamera();

        if (movement != null) movement.enabled = false;
        if (animationController != null) animationController.enabled = false;
        if (playerLook != null) playerLook.enabled = false;
        if (characterController != null) characterController.enabled = false;
        if (animator != null) animator.enabled = false;

        SetRagdollPhysics(true);
        Physics.SyncTransforms();

        impactDirection.y = 0f;
        if (impactDirection.sqrMagnitude < 0.001f)
            impactDirection = -transform.forward;

        Vector3 horizontalImpulse = impactDirection.normalized * Mathf.Max(0f, impactForce);
        hipsBody.AddForce(horizontalImpulse, ForceMode.Impulse);

        foreach (Rigidbody body in ragdollBodies)
        {
            if (body != null)
                body.AddForce(Vector3.up * upwardVelocity, ForceMode.VelocityChange);
        }
        return true;
    }

    public void EndRagdoll()
    {
        if (!IsRagdollActive) return;

        Vector3 recoveryPosition = FindGroundBelowHips();
        StopRagdollBodies();
        SetRagdollPhysics(false);

        transform.SetPositionAndRotation(
            recoveryPosition,
            Quaternion.LookRotation(recoveryForward, Vector3.up)
        );

        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }

        RestoreCamera();

        if (characterController != null) characterController.enabled = true;
        if (movement != null) movement.enabled = movementWasEnabled;
        if (animationController != null) animationController.enabled = animationControllerWasEnabled;
        if (playerLook != null) playerLook.enabled = lookWasEnabled;

        IsRagdollActive = false;
    }

    private void OnDisable()
    {
        if (IsRagdollActive) EndRagdoll();
    }

    private void FindPlayerReferences()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (movement == null) movement = GetComponent<PlayerMovementV3>();
        if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
        if (playerLook == null) playerLook = GetComponent<PlayerLookV2>();

        if (cameraRig == null)
        {
            Camera playerCamera = GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
                cameraRig = playerCamera.transform.parent != null
                    ? playerCamera.transform.parent
                    : playerCamera.transform;
        }
    }

    private void FindRagdollParts()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);

        List<Collider> bodyColliders = new List<Collider>();
        foreach (Collider bodyCollider in GetComponentsInChildren<Collider>(true))
        {
            if (bodyCollider != characterController && bodyCollider.attachedRigidbody != null)
                bodyColliders.Add(bodyCollider);
        }
        ragdollColliders = bodyColliders.ToArray();

        if (hipsBody == null && animator != null && animator.isHuman)
        {
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hips != null) hipsBody = hips.GetComponent<Rigidbody>();
        }
    }

    private void RememberPlayerState()
    {
        movementWasEnabled = movement != null && movement.enabled;
        animationControllerWasEnabled = animationController != null && animationController.enabled;
        lookWasEnabled = playerLook != null && playerLook.enabled;

        recoveryForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (recoveryForward.sqrMagnitude < 0.01f)
            recoveryForward = Vector3.forward;
    }

    private void SetRagdollPhysics(bool active)
    {
        if (ragdollBodies != null)
        {
            foreach (Rigidbody body in ragdollBodies)
            {
                if (body == null) continue;
                body.isKinematic = !active;
                body.useGravity = active;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (Collider bodyCollider in ragdollColliders)
            {
                if (bodyCollider != null)
                    bodyCollider.enabled = active;
            }
        }
    }

    private void StopRagdollBodies()
    {
        if (ragdollBodies == null) return;

        foreach (Rigidbody body in ragdollBodies)
        {
            if (body == null) continue;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private void PrepareCamera()
    {
        if (cameraRig == null || hipsBody == null) return;

        cameraOriginalParent = cameraRig.parent;
        cameraOriginalLocalPosition = cameraRig.localPosition;
        cameraOriginalLocalRotation = cameraRig.localRotation;
        cameraOffsetFromHips = cameraRig.position - hipsBody.position;
        cameraRig.SetParent(null, true);
    }

    private void RestoreCamera()
    {
        if (cameraRig == null) return;

        cameraRig.SetParent(cameraOriginalParent, false);
        cameraRig.localPosition = cameraOriginalLocalPosition;
        cameraRig.localRotation = cameraOriginalLocalRotation;
    }

    private Vector3 FindGroundBelowHips()
    {
        Vector3 recoveryPosition = hipsBody.position;
        RaycastHit hit;

        if (Physics.Raycast(
            hipsBody.position + Vector3.up * 1.5f,
            Vector3.down,
            out hit,
            5f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            recoveryPosition.y = hit.point.y;
        }
        else
        {
            recoveryPosition.y = transform.position.y;
        }

        return recoveryPosition;
    }
}

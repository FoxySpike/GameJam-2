using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlatformRider : MonoBehaviour
{
    [Header("Detección de Plataforma")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float rayDistance = 0.3f;
    [SerializeField] private float rayOriginOffset = 0.1f;

    private CharacterController characterController;
    private Transform currentPlatform;
    private Vector3 lastPlatformPos;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        FollowPlatform();
    }

    private void FollowPlatform()
    {
        Transform detectedPlatform = null;

        Vector3 origin = transform.position + Vector3.up * rayOriginOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, platformLayer))
        {
            detectedPlatform = hit.transform;
        }

        if (detectedPlatform != currentPlatform)
        {
            currentPlatform = detectedPlatform;
            if (currentPlatform != null)
                lastPlatformPos = currentPlatform.position;
        }
        else if (currentPlatform != null)
        {
            Vector3 platformDelta = currentPlatform.position - lastPlatformPos;
            characterController.Move(platformDelta);
            lastPlatformPos = currentPlatform.position;
        }
    }
}
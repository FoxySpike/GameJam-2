using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerLook : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 100f;

    [Header("Horizontal Camera")]
    [SerializeField] private float maxCameraAngle = 60f;

    [Header("Vertical Camera")]
    [SerializeField] private float minLookAngle = -25f;
    [SerializeField] private float maxLookAngle = 25f;

    [SerializeField] private PlayerInputReader inputReader;

    private float cameraRotationX;
    private float cameraRotationY;

    private void Awake()
    {
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (cameraPivot == null)
        {
            Debug.LogError("PlayerLook requires a cameraPivot reference.", this);
            enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (inputReader.GameplayEnabled) Look();
    }

    private void Look()
    {
        Vector2 lookInput = inputReader.LookInput;
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        cameraRotationY += mouseX;

        if (cameraRotationY > maxCameraAngle)
        {
            float rotationAmount = cameraRotationY - maxCameraAngle;

            transform.Rotate(Vector3.up * rotationAmount);

            cameraRotationY = maxCameraAngle;
        }

        if (cameraRotationY < -maxCameraAngle)
        {
            float rotationAmount = cameraRotationY + maxCameraAngle;

            transform.Rotate(Vector3.up * rotationAmount);

            cameraRotationY = -maxCameraAngle;
        }

        cameraRotationX -= mouseY;

        cameraRotationX = Mathf.Clamp(
            cameraRotationX,
            minLookAngle,
            maxLookAngle
        );

        cameraPivot.localRotation = Quaternion.Euler(
            cameraRotationX,
            cameraRotationY,
            0f
        );
    }
}

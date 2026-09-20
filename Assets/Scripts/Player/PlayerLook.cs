using UnityEngine;

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

    private NIS inputActions;

    private Vector2 lookInput;

    private float cameraRotationX;
    private float cameraRotationY;

    private void Awake()
    {
        inputActions = new NIS();

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLookCanceled;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLookCanceled;

        inputActions.Dispose();
    }

    private void Update()
    {
        Look();
    }

    private void OnLook(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }

    private void Look()
    {
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
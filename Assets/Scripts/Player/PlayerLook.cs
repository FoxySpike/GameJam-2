using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 10f;
    private float currentSwayWeight = 0f;

    [Header("Vertical Limits")]
    [SerializeField] private float minLookAngle = -25f;
    [SerializeField] private float maxLookAngle = 25f;

    [Header("Drunk Idle Sway")]
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float swaySpeed = 1f;

    private NIS inputActions;
    private Vector2 lookInput;
    private Vector2 moveInput; // Necesario para saber si el personaje camina

    private float cameraRotationX;
    private float cameraRotationY;

    private void Awake()
    {
        inputActions = new NIS();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => lookInput = Vector2.zero;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void OnDestroy() => inputActions.Dispose();

    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        // 1. Acumular la rotación del ratón
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, minLookAngle, maxLookAngle);

        // 2. Comprobar si el jugador está REALMENTE IDLE (Sin mover ratón Y sin caminar)
        bool isFullyIdle = (lookInput.sqrMagnitude < 0.01f) && (moveInput.sqrMagnitude < 0.01f);

        // 3. Transicionar suavemente el peso del mareo
        float targetSwayWeight = isFullyIdle ? 1f : 0f;
        currentSwayWeight = Mathf.Lerp(currentSwayWeight, targetSwayWeight, Time.deltaTime * 5f);

        // 4. Calcular oscilación
        float finalSwayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount * currentSwayWeight;
        float finalSwayY = Mathf.Cos(Time.time * swaySpeed * 0.8f) * swayAmount * currentSwayWeight;

        // 5. IMPORTANTE: Usamos .rotation (Mundo) en lugar de .localRotation.
        // Esto desacopla la cámara de los giros del cuerpo del personaje.
        cameraPivot.rotation = Quaternion.Euler(cameraRotationX + finalSwayX, cameraRotationY + finalSwayY, 0f);
    }
}
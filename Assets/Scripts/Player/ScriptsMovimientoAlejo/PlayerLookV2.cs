using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerLookV2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
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

    private float cameraRotationX;
    private float cameraRotationY;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (cameraPivot == null)
        {
            Debug.LogError("PlayerLookV2 requiere una referencia a cameraPivot.", this);
            enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        // Si el gameplay está bloqueado por el InputReader, consideramos lecturas en cero
        Vector2 lookInput = inputReader.GameplayEnabled ? inputReader.LookInput : Vector2.zero;
        Vector2 moveInput = inputReader.GameplayEnabled ? inputReader.MoveInput : Vector2.zero;

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
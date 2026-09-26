using UnityEngine;

public class GrillCameraController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("La cámara exclusiva del asador (o el pivote que la mueve)")]
    [SerializeField] private Transform grillCameraPivot;
    [SerializeField] private Camera grillCamera;

    [Header("Configuración de Vista")]
    [SerializeField] private float lookSensitivity = 5f;
    [SerializeField] private float maxLookAngle = 30f; // No dejar que mire hacia atrás

    // Variables internas
    private PlayerInputReader currentInput;
    private float cameraRotationX;
    private float cameraRotationY;

    // Simulación simplificada del Sway de tu PlayerLookV2
    private float swayTimerX, swayTimerY;
    private float swayAmount = 2f;
    private float swaySpeed = 1.5f;

    private void Awake()
    {
        if (grillCamera != null) grillCamera.gameObject.SetActive(false);
    }

    // El minijuego llama a esto al empezar
    public void ActivateCamera(PlayerInputReader inputReader, float borracheraAmount, float borracheraSpeed)
    {
        currentInput = inputReader;
        swayAmount = borracheraAmount;
        swaySpeed = borracheraSpeed;

        // Reseteamos la vista al centro
        cameraRotationX = 0f;
        cameraRotationY = 0f;

        if (grillCamera != null) grillCamera.gameObject.SetActive(true);
    }

    // El minijuego llama a esto al terminar
    public void DeactivateCamera()
    {
        currentInput = null;
        if (grillCamera != null) grillCamera.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentInput == null || !currentInput.GameplayEnabled || currentInput.CurrentContext != PlayerInputReader.InputContext.Grill)
            return;

        Vector2 lookInput = currentInput.GrillLookInput;

        // 1. Movimiento del ratón
        cameraRotationY += lookInput.x * lookSensitivity * Time.deltaTime;
        cameraRotationX -= lookInput.y * lookSensitivity * Time.deltaTime;

        // Limitamos para que el jugador no mire el techo o el suelo y pierda la UI
        cameraRotationX = Mathf.Clamp(cameraRotationX, -maxLookAngle, maxLookAngle);
        cameraRotationY = Mathf.Clamp(cameraRotationY, -maxLookAngle, maxLookAngle);

        // 2. Aplicar el Sway (Balanceo de Borracho)
        swayTimerX += Time.deltaTime * swaySpeed;
        swayTimerY += Time.deltaTime * swaySpeed * 0.8f;

        float finalSwayX = Mathf.Sin(swayTimerX) * swayAmount;
        float finalSwayY = Mathf.Cos(swayTimerY) * swayAmount;

        // 3. Aplicar rotación
        grillCameraPivot.localRotation = Quaternion.Euler(cameraRotationX + finalSwayX, cameraRotationY + finalSwayY, 0f);
    }
}
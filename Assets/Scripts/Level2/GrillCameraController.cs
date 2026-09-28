using UnityEngine;

public class GrillCameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform grillCameraPivot;
    [SerializeField] private Camera grillCamera;

    [Header("Configuración de Vista")]
    [Tooltip("Sensibilidad ajustada para valores de Delta [Mouse] directos")]
    [SerializeField] private float lookSensitivity = 0.15f;

    [Tooltip("Límite vertical (Pitch): Inclinación arriba/abajo")]
    [SerializeField] private float maxPitchAngle = 20f;

    [Tooltip("Límite horizontal (Yaw): Giro izquierda/derecha")]
    [SerializeField] private float maxYawAngle = 35f;

    [Header("Suavizado")]
    [Tooltip("Velocidad de respuesta de la cámara (Valores entre 8 y 15 funcionan bien)")]
    [SerializeField] private float smoothSpeed = 10f;

    // Variables internas
    private PlayerInputReader currentInput;
    private float cameraRotationX;
    private float cameraRotationY;

    private Vector3 initialEulerAngles;

    private float swayTimerX, swayTimerY;
    private float swayAmount = 2f;
    private float swaySpeed = 1.5f;

    private void Awake()
    {
        if (grillCamera != null) grillCamera.gameObject.SetActive(false);

        if (grillCameraPivot != null)
        {
            // Guardamos los ángulos locales iniciales definidos en el Editor para el Pivote
            initialEulerAngles = grillCameraPivot.localEulerAngles;
        }
    }

    public void ActivateCamera(PlayerInputReader inputReader, float borracheraAmount, float borracheraSpeed)
    {
        currentInput = inputReader;
        swayAmount = borracheraAmount;
        swaySpeed = borracheraSpeed;

        // Reiniciamos offsets de rotación y temporizadores de sway al activar
        cameraRotationX = 0f;
        cameraRotationY = 0f;
        swayTimerX = 0f;
        swayTimerY = 0f;

        if (grillCamera != null) grillCamera.gameObject.SetActive(true);
    }

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

        // 1. Acumulación del input
        cameraRotationY += lookInput.x * lookSensitivity;
        cameraRotationX -= lookInput.y * lookSensitivity;

        // 2. Clamping independiente
        cameraRotationX = Mathf.Clamp(cameraRotationX, -maxPitchAngle, maxPitchAngle);
        cameraRotationY = Mathf.Clamp(cameraRotationY, -maxYawAngle, maxYawAngle);

        // 3. Calculamos el Sway (Mareo)
        swayTimerX += Time.deltaTime * swaySpeed;
        swayTimerY += Time.deltaTime * swaySpeed * 0.8f;

        float swayZ = Mathf.Sin(swayTimerX) * swayAmount * 1.5f;
        float swayY = Mathf.Cos(swayTimerY) * swayAmount * 0.5f;
        float swayX = Mathf.Sin(swayTimerX * 0.5f) * (swayAmount * 0.2f);

        // 4. Calculamos la rotación objetivo deseada
        float targetX = initialEulerAngles.x + cameraRotationX + swayX;
        float targetY = initialEulerAngles.y + cameraRotationY + swayY;
        float targetZ = initialEulerAngles.z + swayZ;

        Quaternion targetRotation = Quaternion.Euler(targetX, targetY, targetZ);

        // 5. Interpolación suave (Slerp) para eliminar tirones erráticos
        grillCameraPivot.localRotation = Quaternion.Slerp(
            grillCameraPivot.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}
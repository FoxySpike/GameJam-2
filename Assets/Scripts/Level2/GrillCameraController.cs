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

    // Variables internas de control
    private PlayerInputReader currentInput;
    private float cameraRotationX; // Pitch (Arriba / Abajo)
    private float cameraRotationY; // Yaw (Izquierda / Derecha)

    // Almacenamos la rotación inicial limpia como Quaternion
    private Quaternion initialLocalRotation;

    // Temporizadores y valores para el mareo (Sway)
    private float swayTimerX, swayTimerY;
    private float swayAmount = 2f;
    private float swaySpeed = 1.5f;

    private void Awake()
    {
        if (grillCamera != null)
            grillCamera.gameObject.SetActive(false);

        if (grillCameraPivot != null)
        {
            // Guardamos la rotación inicial como Quaternion para evitar problemas con Euler
            initialLocalRotation = grillCameraPivot.localRotation;
        }
    }

    public void ActivateCamera(PlayerInputReader inputReader, float borracheraAmount, float borracheraSpeed)
    {
        currentInput = inputReader;
        swayAmount = borracheraAmount;
        swaySpeed = borracheraSpeed;

        // Reiniciamos deltas y temporizadores
        cameraRotationX = 0f;
        cameraRotationY = 0f;
        swayTimerX = 0f;
        swayTimerY = 0f;

        if (grillCameraPivot != null)
        {
            initialLocalRotation = grillCameraPivot.localRotation;
        }

        if (grillCamera != null)
            grillCamera.gameObject.SetActive(true);
    }

    public void DeactivateCamera()
    {
        currentInput = null;
        if (grillCamera != null)
            grillCamera.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentInput == null || !currentInput.GameplayEnabled || currentInput.CurrentContext != PlayerInputReader.InputContext.Grill)
            return;

        Vector2 lookInput = currentInput.GrillLookInput;

        // 1. Acumulación y restricción (Clamping) de la entrada del jugador
        cameraRotationY += lookInput.x * lookSensitivity;
        cameraRotationX -= lookInput.y * lookSensitivity;

        cameraRotationX = Mathf.Clamp(cameraRotationX, -maxPitchAngle, maxPitchAngle);
        cameraRotationY = Mathf.Clamp(cameraRotationY, -maxYawAngle, maxYawAngle);

        // 2. Cálculo del mareo (Sway) en offsets locales
        swayTimerX += Time.deltaTime * swaySpeed;
        swayTimerY += Time.deltaTime * swaySpeed * 0.8f;

        float swayPitch = Mathf.Sin(swayTimerX * 0.5f) * (swayAmount * 0.2f);
        float swayYaw = Mathf.Cos(swayTimerY) * (swayAmount * 0.5f);
        float swayRoll = Mathf.Sin(swayTimerX) * (swayAmount * 1.5f);

        // 3. Combinación de deltas (Mirada + Mareo)
        float totalPitch = cameraRotationX + swayPitch;
        float totalYaw = cameraRotationY + swayYaw;
        float totalRoll = swayRoll;

        // 4. Calculamos el Quaternion offset relativo a la postura base
        Quaternion offsetRotation = Quaternion.Euler(totalPitch, totalYaw, totalRoll);
        Quaternion targetRotation = initialLocalRotation * offsetRotation;

        // 5. Interpolación suave (Slerp)
        grillCameraPivot.localRotation = Quaternion.Slerp(
            grillCameraPivot.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}
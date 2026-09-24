using UnityEngine;

public class FridgeHandController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Movement Settings")]
    [SerializeField] private float xySensitivity = 0.005f; // NUEVO: Renombrado para claridad
    [SerializeField] private float zSensitivity = 0.01f;   // NUEVO: Sensibilidad independiente para la profundidad
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Local Boundaries (Límites en espacio local)")]
    // NUEVO: Ahora son Vector3 para incluir el límite de qué tan al fondo (Z max) o qué tan atrás (Z min) puede ir.
    [SerializeField] private Vector3 minLocalBounds = new Vector3(-0.5f, -0.4f, 0f);
    [SerializeField] private Vector3 maxLocalBounds = new Vector3(0.5f, 0.4f, 1.0f);

    [Header("Breath & Shake Mechanics")]
    [SerializeField] private float defaultShakeAmount = 0.02f;
    [SerializeField] private float defaultShakeSpeed = 3.0f;

    private Vector3 targetLocalPosition;
    private Vector3 currentVelocity;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        targetLocalPosition = initialLocalPosition;
    }

    private void OnEnable()
    {
        targetLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (inputReader == null || inputReader.CurrentContext != PlayerInputReader.InputContext.Fridge)
            return;

        HandleHandMovement();
    }

    private void HandleHandMovement()
    {
        // 1. Obtener la entrada del mouse (X,Y) y la de profundidad (Z)
        Vector2 xyInput = inputReader.HandMoveInput;
        float depthInput = inputReader.DepthInput; // NUEVO: Leemos tu nueva variable

        // 2. Acumular el desplazamiento en los 3 ejes
        targetLocalPosition.x += xyInput.x * xySensitivity;
        targetLocalPosition.y += xyInput.y * xySensitivity;
        targetLocalPosition.z += depthInput * zSensitivity; // NUEVO: Mueve en Z

        // 3. Restringir la posición dentro de los límites locales en los 3 ejes (Clamping 3D)
        targetLocalPosition.x = Mathf.Clamp(targetLocalPosition.x, minLocalBounds.x, maxLocalBounds.x);
        targetLocalPosition.y = Mathf.Clamp(targetLocalPosition.y, minLocalBounds.y, maxLocalBounds.y);
        targetLocalPosition.z = Mathf.Clamp(targetLocalPosition.z, minLocalBounds.z, maxLocalBounds.z); // NUEVO

        // 4. Calcular el temblor natural del brazo
        bool isHoldingBreath = inputReader.HoldBreath;
        float currentShakeFactor = isHoldingBreath ? 0.15f : 1.0f;

        Vector3 shakeOffset = CalculateHandShake(currentShakeFactor);

        // 5. Aplicar movimiento suavizado
        Vector3 finalTarget = targetLocalPosition + shakeOffset;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalTarget, ref currentVelocity, smoothTime);
    }

    private Vector3 CalculateHandShake(float shakeFactor)
    {
        float time = Time.time * defaultShakeSpeed;

        float noiseX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * defaultShakeAmount * shakeFactor;
        float noiseY = (Mathf.PerlinNoise(0f, time) - 0.5f) * defaultShakeAmount * shakeFactor;

        // Bonus opcional: Un ligero temblor en Z lo hace sentir más torpe/borracho
        float noiseZ = (Mathf.PerlinNoise(time, time) - 0.5f) * (defaultShakeAmount * 0.5f) * shakeFactor;

        return new Vector3(noiseX, noiseY, noiseZ);
    }

    private void OnDrawGizmosSelected()
    {
        // NUEVO: El Gizmo ahora dibuja un cubo en 3D real para que calibres tu caja de movimiento
        Transform parentTransform = transform.parent != null ? transform.parent : transform;

        Vector3 center = parentTransform.TransformPoint(new Vector3(
            (minLocalBounds.x + maxLocalBounds.x) * 0.5f,
            (minLocalBounds.y + maxLocalBounds.y) * 0.5f,
            (minLocalBounds.z + maxLocalBounds.z) * 0.5f // Centro en Z
        ));

        Vector3 size = new Vector3(
            maxLocalBounds.x - minLocalBounds.x,
            maxLocalBounds.y - minLocalBounds.y,
            maxLocalBounds.z - minLocalBounds.z // Profundidad en Z
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
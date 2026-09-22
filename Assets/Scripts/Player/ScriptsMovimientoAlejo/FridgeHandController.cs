using UnityEngine;

public class FridgeHandController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Movement Settings")]
    [SerializeField] private float sensitivity = 0.005f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Local Boundaries (Límites en espacio local)")]
    [SerializeField] private Vector2 minLocalBounds = new Vector2(-0.5f, -0.4f);
    [SerializeField] private Vector2 maxLocalBounds = new Vector2(0.5f, 0.4f);

    [Header("Breath & Shake Mechanics")]
    [SerializeField] private float defaultShakeAmount = 0.02f;
    [SerializeField] private float defaultShakeSpeed = 3.0f;

    private Vector3 targetLocalPosition;
    private Vector3 currentVelocity;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        // Guardamos la posición inicial de la mano en el espacio local de la nevera
        initialLocalPosition = transform.localPosition;
        targetLocalPosition = initialLocalPosition;
    }

    private void OnEnable()
    {
        // Reiniciamos la posición objetivo al activar el objeto
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
        // 1. Obtener la entrada del mouse en el contexto de la nevera
        Vector2 deltaInput = inputReader.HandMoveInput;

        // 2. Acumular el desplazamiento en la posición local objetivo
        targetLocalPosition.x += deltaInput.x * sensitivity;
        targetLocalPosition.y += deltaInput.y * sensitivity;

        // 3. Restringir la posición dentro de los límites locales (Clamping)
        targetLocalPosition.x = Mathf.Clamp(targetLocalPosition.x, minLocalBounds.x, maxLocalBounds.x);
        targetLocalPosition.y = Mathf.Clamp(targetLocalPosition.y, minLocalBounds.y, maxLocalBounds.y);

        // 4. Calcular el temblor natural del brazo (Si aguanta la respiración, el temblor se reduce)
        bool isHoldingBreath = inputReader.HoldBreath;
        float currentShakeFactor = isHoldingBreath ? 0.15f : 1.0f;

        Vector3 shakeOffset = CalculateHandShake(currentShakeFactor);

        // 5. Aplicar movimiento suavizado (SmoothDamp evita tirones bruscos)
        Vector3 finalTarget = targetLocalPosition + shakeOffset;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalTarget, ref currentVelocity, smoothTime);
    }

    private Vector3 CalculateHandShake(float shakeFactor)
    {
        float time = Time.time * defaultShakeSpeed;

        // Uso de PerlinNoise para generar un temblor orgánico e impredecible
        float noiseX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * defaultShakeAmount * shakeFactor;
        float noiseY = (Mathf.PerlinNoise(0f, time) - 0.5f) * defaultShakeAmount * shakeFactor;

        return new Vector3(noiseX, noiseY, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujamos los límites en la vista de escena para calibrar visualmente
        Transform parentTransform = transform.parent != null ? transform.parent : transform;
        Vector3 center = parentTransform.TransformPoint(new Vector3(
            (minLocalBounds.x + maxLocalBounds.x) * 0.5f,
            (minLocalBounds.y + maxLocalBounds.y) * 0.5f,
            transform.localPosition.z
        ));

        Vector3 size = new Vector3(
            maxLocalBounds.x - minLocalBounds.x,
            maxLocalBounds.y - minLocalBounds.y,
            0.1f
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
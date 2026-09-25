using UnityEngine;

[RequireComponent(typeof(BreathStaminaSystem))]
[RequireComponent(typeof(Rigidbody))]
public class FridgeHandController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;
    private BreathStaminaSystem breathSystem;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float xySensitivity = 0.005f;
    [SerializeField] private float zSensitivity = 0.01f;
    [SerializeField] private float followSpeed = 25f;

    [Tooltip("Velocidad máxima absoluta (m/s) a la que se puede mover el brazo.")]
    [SerializeField] private float maxVelocity = 8f;

    [Tooltip("Tolerancia de penetración. Valores pequeños (ej. 0.01 = 1cm) evitan que el brazo aplaste objetos contra repisas.")]
    [SerializeField] private float maxDesyncDistance = 0.01f; // CORREGIDO: Reducido de 0.05f a 0.01f

    [Header("Local Boundaries")]
    [SerializeField] private Vector3 minLocalBounds = new Vector3(-0.5f, -0.4f, 0f);
    [SerializeField] private Vector3 maxLocalBounds = new Vector3(0.5f, 0.4f, 1.0f);

    [Header("Drunk Sway Mechanics")]
    [SerializeField] private float swayHorizontalAmount = 0.08f;
    [SerializeField] private float swayVerticalAmount = 0.04f;
    [SerializeField] private float swaySpeed = 2.0f;

    private Vector3 virtualLocalPosition;
    private Vector3 targetLocalWithSway;
    private Vector3 currentShakeOffset;

    private void Awake()
    {
        virtualLocalPosition = transform.localPosition;
        breathSystem = GetComponent<BreathStaminaSystem>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        virtualLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (inputReader == null || inputReader.CurrentContext != PlayerInputReader.InputContext.Fridge)
            return;

        HandleInputAndSway();
        SyncVirtualWithPhysical();
    }

    private void FixedUpdate()
    {
        Vector3 targetWorldPosition = transform.parent != null
            ? transform.parent.TransformPoint(targetLocalWithSway)
            : targetLocalWithSway;

        Vector3 displacement = targetWorldPosition - rb.position;
        Vector3 rawVelocity = displacement * followSpeed;

        rb.linearVelocity = Vector3.ClampMagnitude(rawVelocity, maxVelocity);
    }

    private void HandleInputAndSway()
    {
        Vector2 xyInput = inputReader.HandMoveInput;
        float depthInput = inputReader.DepthInput;

        virtualLocalPosition.x += xyInput.x * xySensitivity;
        virtualLocalPosition.y += xyInput.y * xySensitivity;
        virtualLocalPosition.z += depthInput * zSensitivity;

        virtualLocalPosition.x = Mathf.Clamp(virtualLocalPosition.x, minLocalBounds.x, maxLocalBounds.x);
        virtualLocalPosition.y = Mathf.Clamp(virtualLocalPosition.y, minLocalBounds.y, maxLocalBounds.y);
        virtualLocalPosition.z = Mathf.Clamp(virtualLocalPosition.z, minLocalBounds.z, maxLocalBounds.z);

        float currentShakeFactor = breathSystem.ProcessBreathAndGetShakeFactor(inputReader.HoldBreath);

        currentShakeOffset = CalculateHandShake(currentShakeFactor);
        targetLocalWithSway = virtualLocalPosition + currentShakeOffset;
    }

    private void SyncVirtualWithPhysical()
    {
        Vector3 actualLocalPos = transform.parent != null
            ? transform.parent.InverseTransformPoint(rb.position)
            : rb.position;

        Vector3 actualVirtualCenter = actualLocalPos - currentShakeOffset;
        Vector3 desyncVector = virtualLocalPosition - actualVirtualCenter;

        // Si la mano física choca con una repisa, obligamos a la posición virtual a pegarse
        // inmediatamente a la posición física real. Así no se acumula fuerza vertical contra la repisa.
        if (desyncVector.magnitude > maxDesyncDistance)
        {
            virtualLocalPosition = actualVirtualCenter + desyncVector.normalized * maxDesyncDistance;
        }
    }

    private Vector3 CalculateHandShake(float shakeFactor)
    {
        float time = Time.time * swaySpeed;
        float swayX = Mathf.Cos(time) * swayHorizontalAmount * shakeFactor;
        float swayY = Mathf.Sin(time * 2f) * swayVerticalAmount * shakeFactor;
        return new Vector3(swayX, swayY, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Transform parentTransform = transform.parent != null ? transform.parent : transform;
        Vector3 center = parentTransform.TransformPoint(new Vector3(
            (minLocalBounds.x + maxLocalBounds.x) * 0.5f,
            (minLocalBounds.y + maxLocalBounds.y) * 0.5f,
            (minLocalBounds.z + maxLocalBounds.z) * 0.5f
        ));
        Vector3 size = new Vector3(
            maxLocalBounds.x - minLocalBounds.x,
            maxLocalBounds.y - minLocalBounds.y,
            maxLocalBounds.z - minLocalBounds.z
        );
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
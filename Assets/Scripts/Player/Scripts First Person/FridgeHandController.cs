using UnityEngine;

[RequireComponent(typeof(BreathStaminaSystem))]
[RequireComponent(typeof(Rigidbody))]
public class FridgeHandController : MonoBehaviour
{
    [Header("Dependencies")]
    private PlayerInputReader inputReader;
    private BreathStaminaSystem breathSystem;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float xySensitivity = 0.005f;
    [SerializeField] private float followSpeed = 25f;

    [Tooltip("Velocidad máxima absoluta (m/s) a la que se puede mover el brazo.")]
    [SerializeField] private float maxVelocity = 8f;
    [SerializeField] private float maxDesyncDistance = 0.01f;

    [Header("Local Boundaries (X and Y only)")]
    [SerializeField] private Vector2 minLocalBounds = new Vector2(-0.5f, -0.4f);
    [SerializeField] private Vector2 maxLocalBounds = new Vector2(0.5f, 0.4f);

    [Header("Drunk Sway Mechanics")]
    [SerializeField] private float swayHorizontalAmount = 0.08f;
    [SerializeField] private float swayVerticalAmount = 0.04f;
    [SerializeField] private float swaySpeed = 2.0f;

    private Vector3 virtualLocalPosition;
    private Vector3 targetLocalWithSway;
    private Vector3 currentShakeOffset;
    private float fixedZPosition;

    private void Awake()
    {
        virtualLocalPosition = transform.localPosition;
        fixedZPosition = transform.localPosition.z;

        breathSystem = GetComponent<BreathStaminaSystem>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        if (PersistentPlayer.Instance != null)
        {
            inputReader = PersistentPlayer.Instance.InputReader;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] No se encontró el PersistentPlayer. ¿Iniciaste desde la Escena 1?");
        }
    }

    private void OnEnable()
    {
        virtualLocalPosition = transform.localPosition;
        fixedZPosition = transform.localPosition.z;
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

        virtualLocalPosition.x += xyInput.x * xySensitivity;
        virtualLocalPosition.y += xyInput.y * xySensitivity;
        virtualLocalPosition.z = fixedZPosition;

        virtualLocalPosition.x = Mathf.Clamp(virtualLocalPosition.x, minLocalBounds.x, maxLocalBounds.x);
        virtualLocalPosition.y = Mathf.Clamp(virtualLocalPosition.y, minLocalBounds.y, maxLocalBounds.y);

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

        if (desyncVector.magnitude > maxDesyncDistance)
        {
            virtualLocalPosition = actualVirtualCenter + desyncVector.normalized * maxDesyncDistance;
            virtualLocalPosition.z = fixedZPosition;
        }
    }

    private Vector3 CalculateHandShake(float shakeFactor)
    {
        float time = Time.time * swaySpeed;
        float swayX = Mathf.Cos(time) * swayHorizontalAmount * shakeFactor;
        float swayY = Mathf.Sin(time * 2f) * swayVerticalAmount * shakeFactor;
        return new Vector3(swayX, swayY, 0f);
    }

    // --- NUEVO MÉTODO PARA LA OPCIÓN B ---
    public void DisplaceHandForGrab(Vector3 pushVectorWorld)
    {
        // 1. Traducimos el vector 3D global al espacio 2D local de tu brazo
        Vector3 localPush = transform.parent != null
            ? transform.parent.InverseTransformVector(pushVectorWorld)
            : pushVectorWorld;

        // 2. Movemos el control virtual para que no pelee intentando regresar abajo
        virtualLocalPosition += localPush;

        // 3. Respetamos tu regla estricta de la Z fija y los límites de la pantalla
        virtualLocalPosition.z = fixedZPosition;
        virtualLocalPosition.x = Mathf.Clamp(virtualLocalPosition.x, minLocalBounds.x, maxLocalBounds.x);
        virtualLocalPosition.y = Mathf.Clamp(virtualLocalPosition.y, minLocalBounds.y, maxLocalBounds.y);

        // 4. Movemos FÍSICAMENTE el brazo en este mismo frame.
        rb.position += pushVectorWorld;
    }
    // -------------------------------------

    private void OnDrawGizmosSelected()
    {
        Transform parentTransform = transform.parent != null ? transform.parent : transform;
        Vector3 center = parentTransform.TransformPoint(new Vector3(
            (minLocalBounds.x + maxLocalBounds.x) * 0.5f,
            (minLocalBounds.y + maxLocalBounds.y) * 0.5f,
            Application.isPlaying ? fixedZPosition : transform.localPosition.z
        ));
        Vector3 size = new Vector3(
            maxLocalBounds.x - minLocalBounds.x,
            maxLocalBounds.y - minLocalBounds.y,
            0.01f
        );
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
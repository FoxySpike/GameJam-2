using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementV2 : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Drunk Drift Settings")]
    [Tooltip("Ángulo máximo en grados que la borrachera desviará la caminata.")]
    [SerializeField] private float driftAngleMax = 45f; // Lo subimos para que los tirones duelan
    [Tooltip("Frecuencia con la que cambia el patrón de borrachera.")]
    [SerializeField] private float driftSpeed = 0.8f;

    // NUEVO: La curva que nos permitirá "esculpir" la borrachera
    [Tooltip("Eje X: Entrada del Perlin (0 a 1). Eje Y: Desvío final (-1 a 1).")]
    [SerializeField]
    private AnimationCurve driftCurve = new AnimationCurve(
        new Keyframe(0f, -1f),
        new Keyframe(0.5f, 0f),
        new Keyframe(1f, 1f)
    );

    [Header("Steering Correction")]
    [Tooltip("Poder de corrección de las teclas A y D para contrarrestar la borrachera (0.1 a 1.0).")]
    [SerializeField] private float strafeWeight = 0.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private Animator animator;
    private NIS inputActions;

    private Vector2 moveInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        inputActions = new NIS();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    private void OnDestroy() => inputActions.Dispose();

    private void Update()
    {
        MoveAndCorrect();
        UpdateAnimation();
    }

    private void MoveAndCorrect()
    {
        if (moveInput.sqrMagnitude < 0.01f) return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 playerIntent = (camForward * moveInput.y + camRight * (moveInput.x * strafeWeight)).normalized;

        // 1. Obtenemos el ruido crudo (siempre entre 0.0 y 1.0)
        float rawNoise = Mathf.PerlinNoise(Time.time * driftSpeed, 0f);

        // 2. NUEVO: Pasamos el ruido por la curva para transformar su comportamiento.
        // Ahora TÚ decides qué pasa cuando el ruido es 0.6 o 0.2 dibujándolo en el inspector.
        float normalizedDrift = driftCurve.Evaluate(rawNoise);

        float currentDriftAngle = normalizedDrift * driftAngleMax;

        Quaternion driftRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
        Vector3 finalMoveDirection = driftRotation * playerIntent;

        float currentSpeed = inputActions.Player.Sprint.IsPressed() ? sprintSpeed : walkSpeed;
        characterController.Move(finalMoveDirection * currentSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(finalMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        bool isWalking = moveInput != Vector2.zero;
        animator.SetBool("IsWalking", isWalking);
    }
}
using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
public class PlayerMovementV3 : MonoBehaviour
{
    [Serializable]
    private struct PerfilBorrachera
    {
        public NivelBorrachera estado;
        [Min(0.1f)] public float walkSpeed;
        [Min(0.1f)] public float sprintSpeed;
        public float driftAngleMax;
        public float driftSpeed;
        [Range(0f, 1f)] public float strafeWeight;

        public PerfilBorrachera(NivelBorrachera e, float wSpd, float sSpd, float dAngle, float dSpd, float sWeight)
        {
            estado = e; walkSpeed = wSpd; sprintSpeed = sSpd;
            driftAngleMax = dAngle; driftSpeed = dSpd; strafeWeight = sWeight;
        }
    }

    [Header("Perfiles de Movimiento")]
    [SerializeField]
    private PerfilBorrachera[] perfilesEstado =
    {
        new PerfilBorrachera(NivelBorrachera.Sobrio, 3f, 6f, 0f, 0f, 1f),
        new PerfilBorrachera(NivelBorrachera.Prendido, 3f, 5.5f, 20f, 0.7f, 0.8f),
        new PerfilBorrachera(NivelBorrachera.Tomado, 2.5f, 4f, 45f, 1.7f, 0.9f),
        new PerfilBorrachera(NivelBorrachera.VueltoMierda, 2f, 2.5f, 73f, 3f, 1f)
    };

    [Header("Curva de Tambaleo")]
    [SerializeField]
    private AnimationCurve driftCurve = new AnimationCurve(
        new Keyframe(0f, -1f), new Keyframe(0.5f, 0f), new Keyframe(1f, 1f)
    );

    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private AlcoholSystem alcoholSystem;

    [Header("Configuración Extra")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float transitionSpeed = 2f;

    // EVENTO: Notifica a los sistemas externos cuando el estado de movimiento cambia
    public event Action<bool> OnMovingChanged;

    private CharacterController characterController;
    private PerfilBorrachera perfilObjetivo;

    // Semilla aleatoria para cortar una franja neutra en el mapa 2D de Ruido de Perlin
    private float noiseOffsetY;

    private float currentWalkSpeed;
    private float currentSprintSpeed;
    private float currentDriftAngleMax;
    private float currentDriftSpeed;
    private float currentStrafeWeight;

    private bool isMoving;
    public bool IsMoving
    {
        get => isMoving;
        private set
        {
            if (isMoving == value) return; // Si no cambió el estado, no hacemos nada
            isMoving = value;
            OnMovingChanged?.Invoke(isMoving); // Disparamos el evento solo al cambiar
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Generamos un offset en Y aleatorio y lejano para evitar muestrear sobre bordes enteros
        noiseOffsetY = UnityEngine.Random.Range(100f, 10000f);
    }

    private void OnEnable()
    {
        if (alcoholSystem != null)
            alcoholSystem.OnAlcoholStateChanged += SetTargetProfile;

        inputReader.InputAvailabilityChanged += OnInputAvailabilityChanged;

        SetTargetProfile(alcoholSystem != null ? alcoholSystem.CurrentState : NivelBorrachera.Sobrio);
        SnapToTargetProfile();
    }

    private void OnDisable()
    {
        if (alcoholSystem != null)
            alcoholSystem.OnAlcoholStateChanged -= SetTargetProfile;

        inputReader.InputAvailabilityChanged -= OnInputAvailabilityChanged;
    }

    private void Update()
    {
        LerpTowardsTargetProfile();

        // 1. Orientación constante hacia la dirección de la cámara/cruceta
        RotateTowardsCrosshair();

        // 2. Desplazamiento y corrección de la física/borrachera
        MoveAndCorrect();
    }

    private void LerpTowardsTargetProfile()
    {
        currentWalkSpeed = Mathf.Lerp(currentWalkSpeed, perfilObjetivo.walkSpeed, Time.deltaTime * transitionSpeed);
        currentSprintSpeed = Mathf.Lerp(currentSprintSpeed, perfilObjetivo.sprintSpeed, Time.deltaTime * transitionSpeed);
        currentDriftAngleMax = Mathf.Lerp(currentDriftAngleMax, perfilObjetivo.driftAngleMax, Time.deltaTime * transitionSpeed);
        currentDriftSpeed = Mathf.Lerp(currentDriftSpeed, perfilObjetivo.driftSpeed, Time.deltaTime * transitionSpeed);
        currentStrafeWeight = Mathf.Lerp(currentStrafeWeight, perfilObjetivo.strafeWeight, Time.deltaTime * transitionSpeed);
    }

    /// <summary>
    /// Mantiene el cuerpo del personaje siempre orientado hacia la dirección de la cámara.
    /// </summary>
    private void RotateTowardsCrosshair()
    {
        if (cameraTransform == null) return;

        Vector3 aimDirection = cameraTransform.forward;
        aimDirection.y = 0f; // Ignoramos la inclinación vertical para no inclinar el cuerpo hacia el suelo/cielo

        if (aimDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(aimDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Aplica la traslación del personaje basada en el input WASD y la desviación por borrachera.
    /// </summary>
    private void MoveAndCorrect()
    {
        Vector2 moveInput = inputReader.MoveInput;

        if (moveInput.sqrMagnitude < 0.01f || !inputReader.GameplayEnabled)
        {
            IsMoving = false;
            return;
        }

        IsMoving = true;

        // Obtenemos los ejes plano-horizontales de la cámara
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Intención de movimiento relativa a la cámara
        Vector3 playerIntent = (camForward * moveInput.y + camRight * (moveInput.x * currentStrafeWeight)).normalized;

        // Desviación por estado de ebriedad (Ruido de Perlin con offset aleatorio)
        float rawNoise = Mathf.PerlinNoise(Time.time * currentDriftSpeed, noiseOffsetY);

        // Evaluamos en la curva (0.0 -> -1 | 0.5 -> 0 | 1.0 -> +1)
        float normalizedDrift = driftCurve.Evaluate(rawNoise);
        float currentDriftAngle = normalizedDrift * currentDriftAngleMax;

        Quaternion driftRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
        Vector3 finalMoveDirection = driftRotation * playerIntent;

        float currentSpeed = inputReader.Sprint ? currentSprintSpeed : currentWalkSpeed;

        // Traslación física mediante el CharacterController
        characterController.Move(finalMoveDirection * currentSpeed * Time.deltaTime);
    }

    private void SetTargetProfile(NivelBorrachera newState)
    {
        foreach (PerfilBorrachera perfil in perfilesEstado)
        {
            if (perfil.estado == newState)
            {
                perfilObjetivo = perfil;
                break;
            }
        }
    }

    private void SnapToTargetProfile()
    {
        currentWalkSpeed = perfilObjetivo.walkSpeed;
        currentSprintSpeed = perfilObjetivo.sprintSpeed;
        currentDriftAngleMax = perfilObjetivo.driftAngleMax;
        currentDriftSpeed = perfilObjetivo.driftSpeed;
        currentStrafeWeight = perfilObjetivo.strafeWeight;
    }

    private void OnInputAvailabilityChanged()
    {
        if (!inputReader.GameplayEnabled) IsMoving = false;
    }
}
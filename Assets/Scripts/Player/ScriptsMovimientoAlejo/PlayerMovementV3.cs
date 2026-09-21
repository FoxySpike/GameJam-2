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
        public float driftAngleMax; // Ángulo máximo de desvío según el nivel de ebriedad
        [Range(0f, 1f)] public float strafeWeight;

        public PerfilBorrachera(NivelBorrachera e, float wSpd, float sSpd, float dAngle, float sWeight)
        {
            estado = e; walkSpeed = wSpd; sprintSpeed = sSpd;
            driftAngleMax = dAngle; strafeWeight = sWeight;
        }
    }

    [Header("Perfiles de Movimiento")]
    [SerializeField]
    private PerfilBorrachera[] perfilesEstado =
    {
        new PerfilBorrachera(NivelBorrachera.Sobrio, 3f, 6f, 0f, 1f),
        new PerfilBorrachera(NivelBorrachera.Prendido, 3f, 5.5f, 20f, 0.8f),
        new PerfilBorrachera(NivelBorrachera.Tomado, 2.5f, 4f, 45f, 0.9f),
        new PerfilBorrachera(NivelBorrachera.VueltoMierda, 2f, 2.5f, 73f, 1f)
    };

    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private AlcoholSystem alcoholSystem;
    [SerializeField] private PlayerBalanceSystem balanceSystem;

    [Header("Configuración Extra")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float transitionSpeed = 2f;

    public event Action<bool> OnMovingChanged;

    private CharacterController characterController;
    private PerfilBorrachera perfilObjetivo;

    private float currentWalkSpeed;
    private float currentSprintSpeed;
    private float currentDriftAngleMax;
    private float currentStrafeWeight;

    private bool isMoving;
    public bool IsMoving
    {
        get => isMoving;
        private set
        {
            if (isMoving == value) return;
            isMoving = value;
            OnMovingChanged?.Invoke(isMoving);
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();
        if (balanceSystem == null) balanceSystem = GetComponent<PlayerBalanceSystem>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
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
        RotateTowardsCrosshair();
        MoveAndCorrect();
    }

    private void LerpTowardsTargetProfile()
    {
        currentWalkSpeed = Mathf.Lerp(currentWalkSpeed, perfilObjetivo.walkSpeed, Time.deltaTime * transitionSpeed);
        currentSprintSpeed = Mathf.Lerp(currentSprintSpeed, perfilObjetivo.sprintSpeed, Time.deltaTime * transitionSpeed);
        currentDriftAngleMax = Mathf.Lerp(currentDriftAngleMax, perfilObjetivo.driftAngleMax, Time.deltaTime * transitionSpeed);
        currentStrafeWeight = Mathf.Lerp(currentStrafeWeight, perfilObjetivo.strafeWeight, Time.deltaTime * transitionSpeed);
    }

    private void RotateTowardsCrosshair()
    {
        if (cameraTransform == null) return;

        Vector3 aimDirection = cameraTransform.forward;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(aimDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void MoveAndCorrect()
    {
        Vector2 moveInput = inputReader.MoveInput;

        if (moveInput.sqrMagnitude < 0.01f || !inputReader.GameplayEnabled)
        {
            IsMoving = false;
            return;
        }

        IsMoving = true;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 playerIntent = (camForward * moveInput.y + camRight * (moveInput.x * currentStrafeWeight)).normalized;

        // LECTURA DIRECTA DE LA FUENTE DE LA VERDAD
        float balanceOffset = balanceSystem != null ? balanceSystem.CurrentBalance : 0f;

        // CORRECCIÓN DE REVERSA (S):
        // Si el jugador se mueve hacia atrás (moveInput.y < 0), invertimos el ángulo
        // para compensar la rotación del vector negativo en espacio de cámara.
        float reverseMultiplier = moveInput.y < 0f ? -1f : 1f;
        float currentDriftAngle = balanceOffset * currentDriftAngleMax * reverseMultiplier;

        Quaternion driftRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
        Vector3 finalMoveDirection = driftRotation * playerIntent;

        float currentSpeed = inputReader.Sprint ? currentSprintSpeed : currentWalkSpeed;

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
        currentStrafeWeight = perfilObjetivo.strafeWeight;
    }

    private void OnInputAvailabilityChanged()
    {
        if (!inputReader.GameplayEnabled) IsMoving = false;
    }
}
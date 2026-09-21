using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerLookV2 : MonoBehaviour
{
    [Serializable]
    private struct PerfilSwayCamara
    {
        public NivelBorrachera estado;
        public float swayAmount;
        public float swaySpeed;

        public PerfilSwayCamara(NivelBorrachera e, float amount, float speed)
        {
            estado = e;
            swayAmount = amount;
            swaySpeed = speed;
        }
    }

    [Header("Perfiles de Mareo por Borrachera")]
    [SerializeField]
    private PerfilSwayCamara[] perfilesCamara =
    {
        new PerfilSwayCamara(NivelBorrachera.Sobrio, 0f, 0f),
        new PerfilSwayCamara(NivelBorrachera.Prendido, 0.5f, 1f),
        new PerfilSwayCamara(NivelBorrachera.Tomado, 2f, 1.8f),
        new PerfilSwayCamara(NivelBorrachera.VueltoMierda, 4f, 2.5f)
    };

    [Header("Referencias")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private AlcoholSystem alcoholSystem;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 10f;
    [SerializeField] private float transitionSpeed = 2f;

    [Header("Vertical Limits")]
    [SerializeField] private float minLookAngle = -25f;
    [SerializeField] private float maxLookAngle = 25f;

    private float cameraRotationX;
    private float cameraRotationY;

    // Temporizadores de fase independientes para cada eje
    private float swayTimerX;
    private float swayTimerY;

    // Variables activas interpoladas
    private PerfilSwayCamara perfilObjetivo;
    private float currentSwayAmount;
    private float currentSwaySpeed;
    private float currentSwayWeight;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (alcoholSystem == null)
            alcoholSystem = GetComponentInParent<AlcoholSystem>();

        if (cameraPivot == null)
        {
            Debug.LogError("PlayerLookV2 requiere una referencia a cameraPivot.", this);
            enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        if (alcoholSystem != null)
            alcoholSystem.OnAlcoholStateChanged += SetTargetProfile;

        SetTargetProfile(alcoholSystem != null ? alcoholSystem.CurrentState : NivelBorrachera.Sobrio);
        SnapToTargetProfile();
    }

    private void OnDisable()
    {
        if (alcoholSystem != null)
            alcoholSystem.OnAlcoholStateChanged -= SetTargetProfile;
    }

    private void LateUpdate()
    {
        LerpTowardsTargetProfile();
        Look();
    }

    private void LerpTowardsTargetProfile()
    {
        currentSwayAmount = Mathf.Lerp(currentSwayAmount, perfilObjetivo.swayAmount, Time.deltaTime * transitionSpeed);
        currentSwaySpeed = Mathf.Lerp(currentSwaySpeed, perfilObjetivo.swaySpeed, Time.deltaTime * transitionSpeed);
    }

    private void Look()
    {
        Vector2 lookInput = inputReader.GameplayEnabled ? inputReader.LookInput : Vector2.zero;
        Vector2 moveInput = inputReader.GameplayEnabled ? inputReader.MoveInput : Vector2.zero;

        // 1. Acumular la rotación del ratón
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, minLookAngle, maxLookAngle);

        // 2. Comprobar si el jugador está en reposo
        bool isFullyIdle = (lookInput.sqrMagnitude < 0.01f) && (moveInput.sqrMagnitude < 0.01f);

        // 3. Transicionar peso del mareo
        float targetSwayWeight = isFullyIdle ? 1f : 0f;
        currentSwayWeight = Mathf.Lerp(currentSwayWeight, targetSwayWeight, Time.deltaTime * 5f);

        // 4. ACUMULADORES INDEPENDIENTES: Cada eje avanza a su propia velocidad
        swayTimerX += Time.deltaTime * currentSwaySpeed;
        swayTimerY += Time.deltaTime * currentSwaySpeed * 0.8f;

        // Reset individual cuando cada ángulo completa su propio ciclo de 2*PI
        if (swayTimerX > Mathf.PI * 2f) swayTimerX -= Mathf.PI * 2f;
        if (swayTimerY > Mathf.PI * 2f) swayTimerY -= Mathf.PI * 2f;

        // 5. Calcular oscilación suave sin brincos
        float finalSwayX = Mathf.Sin(swayTimerX) * currentSwayAmount * currentSwayWeight;
        float finalSwayY = Mathf.Cos(swayTimerY) * currentSwayAmount * currentSwayWeight;

        // 6. Aplicar rotación global
        cameraPivot.rotation = Quaternion.Euler(cameraRotationX + finalSwayX, cameraRotationY + finalSwayY, 0f);
    }

    private void SetTargetProfile(NivelBorrachera newState)
    {
        foreach (PerfilSwayCamara perfil in perfilesCamara)
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
        currentSwayAmount = perfilObjetivo.swayAmount;
        currentSwaySpeed = perfilObjetivo.swaySpeed;
    }
}
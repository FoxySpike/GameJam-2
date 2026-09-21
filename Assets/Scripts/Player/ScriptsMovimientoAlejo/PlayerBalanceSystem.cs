using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerBalanceSystem : MonoBehaviour
{
    [Serializable]
    private struct PerfilEquilibrio
    {
        public NivelBorrachera estado;
        public float swayForce;          // Fuerza del bamboleo aleatorio
        public float swaySpeed;          // Velocidad del bamboleo
        public float instabilityFactor;  // Aceleración de la gravedad en los bordes

        public PerfilEquilibrio(NivelBorrachera e, float force, float speed, float instability)
        {
            estado = e;
            swayForce = force;
            swaySpeed = speed;
            instabilityFactor = instability;
        }
    }

    [Header("Perfiles de Pérdida de Equilibrio")]
    [SerializeField]
    private PerfilEquilibrio[] perfilesEstado =
    {
        new PerfilEquilibrio(NivelBorrachera.Sobrio, 0f, 0f, 0f),
        new PerfilEquilibrio(NivelBorrachera.Prendido, 0.3f, 1.0f, 0.4f),
        new PerfilEquilibrio(NivelBorrachera.Tomado, 0.6f, 1.5f, 0.8f),
        new PerfilEquilibrio(NivelBorrachera.VueltoMierda, 0.9f, 2.0f, 1.3f)
    };

    [Header("Referencias")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private AlcoholSystem alcoholSystem;

    [Header("Configuración del Control y Sensibilidad")]
    [SerializeField, Tooltip("Poder de corrección base del jugador")]
    private float playerControlPower = 1.8f;

    [SerializeField, Tooltip("Sensibilidad general de la barra")]
    private float balanceSensitivity = 1.0f;

    [SerializeField, Tooltip("Fuerza con la que el cuerpo intenta regresar al centro de forma natural")]
    private float naturalCenterTendency = 0.15f;

    [Header("Mecánica de Tiempo de Gracia / Salvada")]
    [SerializeField, Range(0.5f, 0.9f), Tooltip("A partir de qué punto del slider se activa la ayuda de emergencia")]
    private float dangerZoneThreshold = 0.65f;

    [SerializeField, Tooltip("Multiplicador de fuerza cuando el jugador intenta salvarse en el borde")]
    private float emergencyRecoveryBoost = 2.2f;

    [SerializeField, Tooltip("Multiplicador que reduce la gravedad mientras el jugador intenta corregir en la zona peligrosa")]
    private float gravityDampeningOnCorrecting = 0.3f;

    public float CurrentBalance => currentBalance;

    public event Action<float> OnBalanceChanged;
    public event Action OnPlayerFell;

    private float currentBalance = 0f;
    private PerfilEquilibrio perfilObjetivo;
    private float noiseOffsetY;

    private float currentSwayForce;
    private float currentSwaySpeed;
    private float currentInstability;

    private float smoothedSwayForce;

    private void Awake()
    {
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();

        noiseOffsetY = UnityEngine.Random.Range(100f, 10000f);
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

    private void Update()
    {
        LerpTowardsTargetProfile();
        CalculateBalance();
    }

    private void CalculateBalance()
    {
        Vector2 moveInput = inputReader.GameplayEnabled ? inputReader.MoveInput : Vector2.zero;
        float rawInputX = moveInput.x;

        // 1. Ruido de Borrachera (Bamboleo Reactivo)
        float rawNoise = Mathf.PerlinNoise(Time.time * currentSwaySpeed, noiseOffsetY);
        float normalizedNoise = (rawNoise - 0.5f) * 2f; // Convertir rango de [0,1] a [-1,1]

        // Transición más rápida del ruido para mantener al jugador en alerta constante
        smoothedSwayForce = Mathf.Lerp(smoothedSwayForce, normalizedNoise * currentSwayForce, Time.deltaTime * 8.0f);

        // 2. Detección de Estado y Ayuda en Zona de Peligro
        float absoluteBalance = Mathf.Abs(currentBalance);
        float balanceSign = Mathf.Sign(currentBalance);

        bool isCorrectingOpposite = (currentBalance > 0f && rawInputX < 0f) || (currentBalance < 0f && rawInputX > 0f);
        bool isInDangerZone = absoluteBalance >= dangerZoneThreshold;

        float effectiveGravityFactor = currentInstability;
        float effectivePlayerPower = playerControlPower;

        if (isInDangerZone && isCorrectingOpposite)
        {
            effectiveGravityFactor *= gravityDampeningOnCorrecting;
            effectivePlayerPower *= emergencyRecoveryBoost;
        }

        // 3. Gravedad Híbrida (Combinación Lineal + Cuadrática para romper la inercia del centro)
        // (0.3 * x) asegura desequilibrio inmediato; (0.7 * x^2) aporta la aceleración peligrosa en bordes.
        float gravityCurve = (0.3f * absoluteBalance) + (0.7f * absoluteBalance * absoluteBalance);
        float gravityForce = gravityCurve * balanceSign * effectiveGravityFactor;

        // 4. Tendencia Natural al Centro (Se reduce a medida que aumenta la inclinación)
        float centerPullDampening = 1f - absoluteBalance;
        float centerPull = -currentBalance * centerPullDampening * naturalCenterTendency;

        // 5. Fuerza del Jugador
        float playerForce = rawInputX * effectivePlayerPower;

        // 6. Integración del Torque Neto
        float netTorque = smoothedSwayForce + centerPull + gravityForce + playerForce;

        currentBalance += netTorque * balanceSensitivity * Time.deltaTime;
        currentBalance = Mathf.Clamp(currentBalance, -1f, 1f);

        OnBalanceChanged?.Invoke(currentBalance);

        if (Mathf.Abs(currentBalance) >= 1f)
        {
            OnPlayerFell?.Invoke();
        }
    }

    private void LerpTowardsTargetProfile()
    {
        currentSwayForce = Mathf.Lerp(currentSwayForce, perfilObjetivo.swayForce, Time.deltaTime * 2f);
        currentSwaySpeed = Mathf.Lerp(currentSwaySpeed, perfilObjetivo.swaySpeed, Time.deltaTime * 2f);
        currentInstability = Mathf.Lerp(currentInstability, perfilObjetivo.instabilityFactor, Time.deltaTime * 2f);
    }

    private void SetTargetProfile(NivelBorrachera newState)
    {
        foreach (PerfilEquilibrio perfil in perfilesEstado)
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
        currentSwayForce = perfilObjetivo.swayForce;
        currentSwaySpeed = perfilObjetivo.swaySpeed;
        currentInstability = perfilObjetivo.instabilityFactor;
    }
}
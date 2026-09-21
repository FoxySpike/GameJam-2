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
        [Range(0f, 0.6f)]
        public float maxIdleSway;        // Límite máximo de balanceo seguro en reposo

        public PerfilEquilibrio(NivelBorrachera e, float force, float speed, float instability, float idleSway)
        {
            estado = e;
            swayForce = force;
            swaySpeed = speed;
            instabilityFactor = instability;
            maxIdleSway = idleSway;
        }
    }

    [Header("Perfiles de Pérdida de Equilibrio")]
    [SerializeField]
    private PerfilEquilibrio[] perfilesEstado =
    {
        new PerfilEquilibrio(NivelBorrachera.Sobrio, 0f, 0f, 0f, 0f),
        new PerfilEquilibrio(NivelBorrachera.Prendido, 0.2f, 0.8f, 0.35f, 0.15f),
        new PerfilEquilibrio(NivelBorrachera.Tomado, 0.4f, 1.0f, 0.65f, 0.30f),
        new PerfilEquilibrio(NivelBorrachera.VueltoMierda, 0.55f, 1.2f, 0.95f, 0.45f)
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
    private float naturalCenterTendency = 0.25f;

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
    private float currentMaxIdleSway;

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
        bool isPlayerMoving = moveInput.sqrMagnitude > 0.01f;

        // 1. Ruido aleatorio del alcohol (Bamboleo)
        float rawNoise = Mathf.PerlinNoise(Time.time * currentSwaySpeed, noiseOffsetY);
        float normalizedNoise = (rawNoise - 0.5f) * 2f;
        float targetBorracheraForce = normalizedNoise * currentSwayForce;

        smoothedSwayForce = Mathf.Lerp(smoothedSwayForce, targetBorracheraForce, Time.deltaTime * 4.0f);

        // 2. Transición IDLE Seguro vs Inercia Inestable
        float absoluteBalance = Mathf.Abs(currentBalance);
        bool isWithinIdleLimits = absoluteBalance <= currentMaxIdleSway;

        // Solo entra a Idle Seguro si el jugador NO presiona nada Y está dentro de los límites de seguridad
        if (!isPlayerMoving && isWithinIdleLimits)
        {
            currentBalance = Mathf.MoveTowards(currentBalance, smoothedSwayForce * currentMaxIdleSway, Time.deltaTime * balanceSensitivity);
            currentBalance = Mathf.Clamp(currentBalance, -currentMaxIdleSway, currentMaxIdleSway);

            OnBalanceChanged?.Invoke(currentBalance);
            return;
        }

        // 3. Cálculo de Gravedad y Detección de Salvada en Zona de Peligro
        // (Aplica si se mueve O si se quedó quieto fuera del límite seguro)
        float balanceSign = Mathf.Sign(currentBalance);

        // ¿El jugador está presionando la tecla opuesta a la inclinación actual?
        bool isCorrectingOpposite = (currentBalance > 0f && rawInputX < 0f) || (currentBalance < 0f && rawInputX > 0f);
        bool isInDangerZone = absoluteBalance >= dangerZoneThreshold;

        float effectiveGravityFactor = currentInstability;
        float effectivePlayerPower = playerControlPower;

        // APLICACIÓN DEL TIEMPO DE GRACIA / RECOVERY BOOST
        if (isInDangerZone && isCorrectingOpposite)
        {
            effectiveGravityFactor *= gravityDampeningOnCorrecting;
            effectivePlayerPower *= emergencyRecoveryBoost;
        }

        // 4. Gravedad Exponencial
        float gravityForce = (currentBalance * currentBalance) * balanceSign * effectiveGravityFactor;

        // 5. Tendencia Natural al Centro
        float centerPull = -currentBalance * naturalCenterTendency;

        // 6. Fuerza del Jugador (Si está quieto sin tocar teclas, rawInputX será 0)
        float playerForce = rawInputX * effectivePlayerPower;

        // 7. Torque Neto Total
        float netTorque = smoothedSwayForce + centerPull + gravityForce + playerForce;

        // 8. Integración del estado final
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
        currentMaxIdleSway = Mathf.Lerp(currentMaxIdleSway, perfilObjetivo.maxIdleSway, Time.deltaTime * 2f);
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
        currentMaxIdleSway = perfilObjetivo.maxIdleSway;
    }
}
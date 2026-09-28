using System;
using UnityEngine;

[RequireComponent(typeof(GrillCameraController))]
public class GrillMinigameController : MonoBehaviour
{
    public event Action OnMinigameWon;
    public event Action OnMinigameLost;

    [Header("Referencias")]
    [SerializeField] private GrillCameraController cameraController;
    [SerializeField] private GameObject uiCanvas3D;

    [Header("Mecánicas Base (Inputs y Tiempo)")]
    [SerializeField] private float maxCookingTime = 20f;

    [Tooltip("La fuerza con la que la temperatura sube o baja naturalmente")]
    [SerializeField] private float baseEnvironmentalDrift = 6f;
    [Tooltip("Cada cuántos segundos cambia la tendencia (de enfriarse a calentarse)")]
    [SerializeField] private float driftChangeInterval = 4f;

    [Tooltip("Fuerza base del jugador dentro de la zona segura (40-60)")]
    [SerializeField] private float baseHeatUpRate = 70f;
    [SerializeField] private float baseActiveCooldownRate = 70f;

    [Tooltip("Multiplicador de potencia cuando la aguja entra en zona de peligro (<40 o >60)")]
    [SerializeField] private float rescueMultiplier = 1.8f;

    [Header("Motor de Caos (Fluctuaciones Aleatorias)")]
    [SerializeField] private float minFluctuationRate = 10f;
    [SerializeField] private float maxFluctuationRate = 25f;
    [SerializeField] private float minFluctuationDuration = 0.5f;
    [SerializeField] private float maxFluctuationDuration = 1.8f;

    private float timeInSweetSpot = 0f;
    public float CurrentHeat => currentHeat;
    public float CurrentProgress => currentProgress;
    public float MaxCookingTime => maxCookingTime;

    private PlayerInputReader inputReader;
    private bool isPlaying = false;

    private float currentHeat = 0f;
    private float currentProgress = 0f;

    private float currentFluctuationTimer = 0f;
    private float activeFluctuationRate = 0f;
    private float nextFluctuationTime = 0f;

    // Variables de estado para la tendencia ambiental
    private float currentDriftRate = 0f;
    private float nextDriftChangeTime = 0f;

    private const float SWEET_SPOT_MIN = 40f;
    private const float SWEET_SPOT_MAX = 60f;

    private void Awake()
    {
        if (cameraController == null) cameraController = GetComponent<GrillCameraController>();
    }

    public void StartMinigame(PlayerInputReader playerInput)
    {
        inputReader = playerInput;

        if (inputReader != null)
        {
            inputReader.SetContext(PlayerInputReader.InputContext.Grill);
        }

        isPlaying = true;
        currentHeat = 50f;
        currentProgress = 0f;
        timeInSweetSpot = 0f;

        currentFluctuationTimer = 0f;
        activeFluctuationRate = 0f;

        // Iniciamos la tendencia (Por costumbre, que empiece enfriándose)
        currentDriftRate = -baseEnvironmentalDrift;
        nextDriftChangeTime = Time.time + driftChangeInterval;

        if (uiCanvas3D != null) uiCanvas3D.SetActive(true);
        cameraController.ActivateCamera(inputReader, 3f, 1.5f);

        ScheduleNextFluctuation();
    }

    private void Update()
    {
        if (!isPlaying || inputReader == null) return;

        HandleEnvironmentalDrift();
        HandleTemperature();
        HandleCookingProgress();
        HandleFluctuations();
    }

    private void HandleEnvironmentalDrift()
    {
        // Cambiamos la tendencia cada ciertos segundos
        if (Time.time >= nextDriftChangeTime)
        {
            // 50% de probabilidad de que la tendencia sea calentarse o enfriarse
            float direction = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            currentDriftRate = baseEnvironmentalDrift * direction;

            // Programamos el siguiente cambio con un poco de variación (+/- 20%)
            float randomVariation = UnityEngine.Random.Range(0.8f, 1.2f);
            nextDriftChangeTime = Time.time + (driftChangeInterval * randomVariation);
        }
    }

    private void HandleTemperature()
    {
        float heatInput = inputReader.GrillHeatControl;

        // 1. Aplicamos la tendencia actual en lugar de un enfriamiento fijo
        currentHeat += currentDriftRate * Time.deltaTime;

        // 2. Determinar si la aguja está en zona de peligro
        bool isInDangerZone = currentHeat < SWEET_SPOT_MIN || currentHeat > SWEET_SPOT_MAX;
        float currentMultiplier = isInDangerZone ? rescueMultiplier : 1.0f;

        // 3. Aplicar input con multiplicador dinámico
        if (heatInput > 0)
        {
            currentHeat += (baseHeatUpRate * currentMultiplier) * Time.deltaTime;
        }
        else if (heatInput < 0)
        {
            currentHeat -= (baseActiveCooldownRate * currentMultiplier) * Time.deltaTime;
        }

        // 4. Aplicar evento de caos
        if (currentFluctuationTimer > 0)
        {
            currentHeat += activeFluctuationRate * Time.deltaTime;
            currentFluctuationTimer -= Time.deltaTime;
        }

        currentHeat = Mathf.Clamp(currentHeat, 0f, 100f);

        if (currentHeat <= 0f || currentHeat >= 100f)
        {
            EndMinigame(isVictory: false);
        }
    }

    private void HandleCookingProgress()
    {
        currentProgress += Time.deltaTime;

        if (currentHeat >= SWEET_SPOT_MIN && currentHeat <= SWEET_SPOT_MAX)
        {
            timeInSweetSpot += Time.deltaTime;
        }

        if (currentProgress >= maxCookingTime)
        {
            bool isNeedleInRed = currentHeat < SWEET_SPOT_MIN || currentHeat > SWEET_SPOT_MAX;
            bool isCookedEnough = timeInSweetSpot >= (maxCookingTime * 0.65f);

            if (isNeedleInRed || !isCookedEnough) EndMinigame(isVictory: false);
            else EndMinigame(isVictory: true);
        }
    }

    private void HandleFluctuations()
    {
        if (currentFluctuationTimer <= 0 && Time.time >= nextFluctuationTime)
        {
            GenerateRandomFluctuation();
            ScheduleNextFluctuation();
        }
    }

    private void GenerateRandomFluctuation()
    {
        currentFluctuationTimer = UnityEngine.Random.Range(minFluctuationDuration, maxFluctuationDuration);
        float intensity = UnityEngine.Random.Range(minFluctuationRate, maxFluctuationRate);

        float chanceToPushUp = 0.5f;

        if (currentHeat < 35f)
        {
            chanceToPushUp = 0.85f;
        }
        else if (currentHeat > 65f)
        {
            chanceToPushUp = 0.15f;
        }

        float directionMultiplier = UnityEngine.Random.value <= chanceToPushUp ? 1f : -1f;
        activeFluctuationRate = intensity * directionMultiplier;
    }

    private void ScheduleNextFluctuation()
    {
        nextFluctuationTime = Time.time + currentFluctuationTimer + UnityEngine.Random.Range(1f, 2.5f);
    }

    private void EndMinigame(bool isVictory)
    {
        isPlaying = false;
        cameraController.DeactivateCamera();
        inputReader = null;

        if (isVictory) OnMinigameWon?.Invoke();
        else OnMinigameLost?.Invoke();
    }
}
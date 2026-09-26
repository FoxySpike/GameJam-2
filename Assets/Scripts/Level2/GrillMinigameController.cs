using System;
using UnityEngine;

[RequireComponent(typeof(GrillCameraController))]
public class GrillMinigameController : MonoBehaviour
{
    // Eventos para notificar el resultado (Decoupled architecture)
    public event Action OnMinigameWon;
    public event Action OnMinigameLost;

    [Header("Referencias")]
    [SerializeField] private GrillCameraController cameraController;
    [SerializeField] private GameObject uiCanvas3D;

    [Header("Mecánicas del Asador")]
    [SerializeField] private float passiveCoolingRate = 15f;
    [SerializeField] private float heatUpAmount = 5f;
    [SerializeField] private float cooldownRate = 30f;
    [SerializeField] private float maxCookingTime = 10f;

    // Propiedades públicas de solo lectura para la UI
    public float CurrentHeat => currentHeat;
    public float CurrentProgress => currentProgress;
    public float MaxCookingTime => maxCookingTime;

    private PlayerInputReader inputReader;
    private bool isPlaying = false;

    private float currentHeat = 0f;
    private float currentProgress = 0f;

    private const float SWEET_SPOT_MIN = 40f;
    private const float SWEET_SPOT_MAX = 60f;

    private float nextFlareUpTime = 0f;

    private void Awake()
    {
        if (cameraController == null) cameraController = GetComponent<GrillCameraController>();
        if (uiCanvas3D != null) uiCanvas3D.SetActive(false);
    }

    public void StartMinigame(PlayerInputReader playerInput)
    {
        inputReader = playerInput;
        isPlaying = true;
        currentHeat = 50f;
        currentProgress = 0f;

        if (uiCanvas3D != null) uiCanvas3D.SetActive(true);

        cameraController.ActivateCamera(inputReader, 3f, 1.5f);

        ScheduleNextFlareUp();
    }

    private void Update()
    {
        if (!isPlaying || inputReader == null) return;

        HandleTemperature();
        HandleCookingProgress();
        HandleFlareUps();
    }

    private void HandleTemperature()
    {
        float heatInput = inputReader.GrillHeatControl;

        // La temperatura cae constantemente
        currentHeat -= passiveCoolingRate * Time.deltaTime;

        if (heatInput > 0)
        {
            currentHeat += heatUpAmount * Time.deltaTime * 10f;
        }
        else if (heatInput < 0)
        {
            currentHeat -= cooldownRate * Time.deltaTime;
        }

        currentHeat = Mathf.Clamp(currentHeat, 0f, 100f);

        // Si la temperatura llega a los extremos (0 o 100), se arruina el pollo
        if (currentHeat <= 0f || currentHeat >= 100f)
        {
            EndMinigame(isVictory: false);
        }
    }

    private void HandleCookingProgress()
    {
        if (currentHeat >= SWEET_SPOT_MIN && currentHeat <= SWEET_SPOT_MAX)
        {
            currentProgress += Time.deltaTime;

            if (currentProgress >= maxCookingTime)
            {
                EndMinigame(isVictory: true);
            }
        }
    }

    private void HandleFlareUps()
    {
        if (Time.time >= nextFlareUpTime)
        {
            currentHeat += 25f;
            Debug.Log("¡LLAMARADA!");
            ScheduleNextFlareUp();
        }
    }

    private void ScheduleNextFlareUp()
    {
        nextFlareUpTime = Time.time + UnityEngine.Random.Range(2f, 5f);
    }

    private void EndMinigame(bool isVictory)
    {
        isPlaying = false;

        if (uiCanvas3D != null) uiCanvas3D.SetActive(false);
        cameraController.DeactivateCamera();

        inputReader = null;

        // Disparamos los eventos correspondientes
        if (isVictory)
        {
            OnMinigameWon?.Invoke();
        }
        else
        {
            OnMinigameLost?.Invoke(); // Dispara el evento de fallo
        }
    }
}
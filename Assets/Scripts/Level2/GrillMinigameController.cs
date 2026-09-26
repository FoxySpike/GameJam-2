using System;
using UnityEngine;

[RequireComponent(typeof(GrillCameraController))]
public class GrillMinigameController : MonoBehaviour
{
    // Eventos para avisar al GrillInteractable
    public event Action OnMinigameWon;
    public event Action OnMinigameLost;

    [Header("Referencias")]
    [SerializeField] private GrillCameraController cameraController;
    [Tooltip("El Canvas 3D que contiene las barras")]
    [SerializeField] private GameObject uiCanvas3D;

    [Header("Mecánicas del Asador")]
    [SerializeField] private float maxCookingTime = 10f; // Segundos para cocinar
    [SerializeField] private float passiveCoolingRate = 15f; // Cuánto baja la aguja sola por segundo
    [SerializeField] private float heatUpAmount = 5f; // Cuánto sube al presionar D
    [SerializeField] private float cooldownRate = 30f; // Cuánto baja al mantener A

    private PlayerInputReader inputReader;
    private bool isPlaying = false;

    // Estado del minijuego
    private float currentHeat = 0f; // Va de 0 a 100
    private float currentProgress = 0f; // Va de 0 a maxCookingTime

    // Zonas de temperatura
    private const float SWEET_SPOT_MIN = 40f;
    private const float SWEET_SPOT_MAX = 60f;

    // Llamaradas
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
        currentHeat = 50f; // Empezamos en el centro
        currentProgress = 0f;

        if (uiCanvas3D != null) uiCanvas3D.SetActive(true);

        // Idealmente, obtendrías estos valores de tu AlcoholSystem
        cameraController.ActivateCamera(inputReader, 3f, 1.5f);

        inputReader.ExitGrill += SurrenderMinigame;

        ScheduleNextFlareUp();
    }

    private void Update()
    {
        if (!isPlaying || inputReader == null) return;

        HandleTemperature();
        HandleCookingProgress();
        HandleFlareUps();

        // AQUÍ DEBES ACTUALIZAR TUS BARRAS 3D VISUALES
        // progressBarUI.fillAmount = currentProgress / maxCookingTime;
        // sweetSpotUI_Aguja.rotation = ... (basado en currentHeat)
    }

    private void HandleTemperature()
    {
        float heatInput = inputReader.GrillHeatControl;

        // La temperatura siempre cae pasivamente (el asador está dañado)
        currentHeat -= passiveCoolingRate * Time.deltaTime;

        if (heatInput > 0)
        {
            // Presionó D: Subimos temperatura a golpes (requiere Spamear si usaste un "Tap" en el Input)
            // Si el input es continuo (mantiene presionado D), sumamos con deltaTime
            currentHeat += heatUpAmount * Time.deltaTime * 10f;
        }
        else if (heatInput < 0)
        {
            // Presionó A: Enfriamos drásticamente
            currentHeat -= cooldownRate * Time.deltaTime;
        }

        currentHeat = Mathf.Clamp(currentHeat, 0f, 100f);

        // Perder por dejar que se apague o se queme al máximo
        if (currentHeat <= 0f || currentHeat >= 100f)
        {
            EndMinigame(false);
        }
    }

    private void HandleCookingProgress()
    {
        // Solo avanza el tiempo si la aguja está en la zona verde
        if (currentHeat >= SWEET_SPOT_MIN && currentHeat <= SWEET_SPOT_MAX)
        {
            currentProgress += Time.deltaTime;

            if (currentProgress >= maxCookingTime)
            {
                EndMinigame(true); // ¡El pollo está listo!
            }
        }
    }

    private void HandleFlareUps()
    {
        if (Time.time >= nextFlareUpTime)
        {
            // ¡Llamarada! Sube la temperatura de golpe
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

        if (inputReader != null)
        {
            inputReader.ExitGrill -= SurrenderMinigame;
            inputReader = null;
        }

        if (isVictory) OnMinigameWon?.Invoke();
        else OnMinigameLost?.Invoke();
    }

    private void SurrenderMinigame()
    {
        EndMinigame(false);
    }
}
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireAlarm : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GrillMinigameController minigameController;

    [Header("Configuración de Alarma")]
    [Tooltip("Cantidad de ruido por segundo que genera la alarma encendida")]
    [SerializeField] private float noisePerSecond = 35f;
    [SerializeField] private AudioClip alarmLoopSound;

    private AudioSource audioSource;
    private bool isAlarmActive = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // Que el sonido se repita automáticamente
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento de derrota del minijuego
        if (minigameController != null)
        {
            minigameController.OnMinigameLost += StartAlarm;
        }
    }

    private void OnDisable()
    {
        // Siempre desuscribirse para evitar errores de memoria (Memory Leaks)
        if (minigameController != null)
        {
            minigameController.OnMinigameLost -= StartAlarm;
        }
    }

    private void Update()
    {
        if (!isAlarmActive) return;

        // Mientras la alarma esté encendida, alimentamos la barra de ruido frame a frame
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.AddNoise(noisePerSecond * Time.deltaTime);
        }
    }

    public void StartAlarm()
    {
        if (isAlarmActive) return;

        isAlarmActive = true;
        Debug.Log("🔥 ¡ALARMA DE FUEGO ACTIVADA! El pollo se quemó.");

        if (alarmLoopSound != null)
        {
            audioSource.clip = alarmLoopSound;
            audioSource.Play();
        }
    }

    public void StopAlarm()
    {
        isAlarmActive = false;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireAlarm : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GrillMinigameController minigameController;

    [Tooltip("Arrastra aquí el componente Light (Point Light o Spot Light)")]
    [SerializeField] private Light alarmLight; // SOLAMENTE LA LUZ

    [Header("Configuración de Alarma")]
    [Tooltip("Cantidad de ruido por segundo que genera la alarma encendida")]
    [SerializeField] private float noisePerSecond = 35f;
    [SerializeField] private float flashSpeed = 8f; // Velocidad del parpadeo
    [SerializeField] private AudioClip alarmLoopSound;

    private AudioSource audioSource;
    private bool isAlarmActive = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // Que el sonido se repita automáticamente

        // Aseguramos que la luz empiece apagada
        if (alarmLight != null) alarmLight.enabled = false;
    }

    private void OnEnable()
    {
        if (minigameController != null)
            minigameController.OnMinigameEnded += CheckForAlarm;
    }

    private void OnDisable()
    {
        if (minigameController != null)
            minigameController.OnMinigameEnded -= CheckForAlarm;
    }

    private void CheckForAlarm(GrillMinigameController.CookingResult result)
    {
        if (result == GrillMinigameController.CookingResult.Burned)
        {
            StartAlarm();
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

        // Parpadeo puramente de la Luz usando una onda matemática
        if (alarmLight != null)
        {
            alarmLight.enabled = Mathf.Sin(Time.time * flashSpeed) > 0;
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

        // Apagamos la luz al detener la alarma
        if (alarmLight != null) alarmLight.enabled = false;
    }
}
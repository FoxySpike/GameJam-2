using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BottleProp : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Tilt Settings (Caerse de lado)")]
    [Tooltip("Ángulo a partir del cual consideramos que la botella se acostó")]
    [SerializeField] private float fallAngleThreshold = 60f;
    [Tooltip("Ángulo para considerar que volvió a estar de pie")]
    [SerializeField] private float uprightAngleThreshold = 30f;

    // Eliminamos las variables específicas de tilt, usaremos las del impacto medio
    private bool isTilted = false;

    [Header("Impact Thresholds (Velocidades)")]
    [SerializeField] private float lightImpactVelocity = 1.5f;
    [SerializeField] private float mediumImpactVelocity = 3.5f;
    [SerializeField] private float hardImpactVelocity = 6.0f;

    [Header("Impact Audio Clips")]
    [SerializeField] private AudioClip lightImpactClip;
    [SerializeField] private AudioClip mediumImpactClip; // Servirá también cuando se caiga
    [SerializeField] private AudioClip hardImpactClip;

    [Header("Noise Amounts (Para el NoiseManager)")]
    [SerializeField] private float lightImpactNoise = 8f;
    [SerializeField] private float mediumImpactNoise = 15f; // Servirá también cuando se caiga
    [SerializeField] private float hardImpactNoise = 30f;

    [Header("Anti-Spam Settings")]
    [SerializeField] private float impactCooldown = 0.25f;
    private float lastImpactTime = -999f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        float currentAngle = Vector3.Angle(transform.up, Vector3.up);

        // Si NO está acostada, revisamos si se acaba de caer
        if (!isTilted && currentAngle > fallAngleThreshold)
        {
            isTilted = true;

            // Asumimos que caerse es equivalente a un "Impacto Medio"
            Debug.Log("[BottleProp] Se cayó de lado (Medio)");
            PlaySound(mediumImpactClip, 0.8f);
            NotifyNoise(mediumImpactNoise);
        }
        // Si SÍ está acostada, revisamos si el jugador la volvió a poner de pie
        else if (isTilted && currentAngle < uprightAngleThreshold)
        {
            // Reseteamos el estado para que pueda volver a sonar si se cae otra vez
            isTilted = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastImpactTime < impactCooldown) return;

        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < lightImpactVelocity) return; // Si es muy suave, lo ignoramos

        lastImpactTime = Time.time;
        Debug.Log($"[BottleProp] Fuerza: {impactForce}");

        // IMPORTANTE: Evaluamos de MAYOR a MENOR.
        if (impactForce >= hardImpactVelocity)
        {
            Debug.Log("-> IMPACTO FUERTE");
            PlaySound(hardImpactClip, 1.0f);
            NotifyNoise(hardImpactNoise);
        }
        else if (impactForce >= mediumImpactVelocity)
        {
            Debug.Log("-> IMPACTO MEDIO");
            PlaySound(mediumImpactClip, 0.7f);
            NotifyNoise(mediumImpactNoise);
        }
        else // Si llegó aquí, es mayor que light pero menor que medium
        {
            Debug.Log("-> IMPACTO LEVE");
            PlaySound(lightImpactClip, 0.5f);
            NotifyNoise(lightImpactNoise);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            // Pequeño truco: Variar ligeramente el pitch hace que el sonido no canse el oído
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // Encapsulamos la llamada al Singleton para no duplicar código
    private void NotifyNoise(float noiseAmount)
    {
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.AddNoise(noiseAmount);
        }
    }
}
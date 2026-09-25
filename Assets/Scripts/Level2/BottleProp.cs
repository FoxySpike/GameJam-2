using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BottleProp : MonoBehaviour
{
    [Header("Dependencies")]
    // Eliminamos la variable noiseManager. Ya no la necesitamos.
    private AudioSource audioSource;

    [Header("Tilt / Fall Settings")]
    [Tooltip("Ángulo a partir del cual consideramos que la botella se acostó")]
    [SerializeField] private float fallAngleThreshold = 60f;
    [SerializeField] private float tiltNoiseAmount = 15f;
    [SerializeField] private AudioClip tiltClip;

    [Header("Impact Settings")]
    [Tooltip("Velocidad mínima para considerar un golpecito suave (ignora la fricción al rodar)")]
    [SerializeField] private float minImpactVelocity = 1.5f;
    [Tooltip("Velocidad a partir de la cual se considera un impacto fuerte (caída desde alto)")]
    [SerializeField] private float hardImpactVelocity = 5.0f;

    [SerializeField] private float lightImpactNoise = 8f;
    [SerializeField] private float hardImpactNoise = 30f;

    [SerializeField] private AudioClip lightImpactClip;
    [SerializeField] private AudioClip hardImpactClip;

    [Header("Anti-Spam Settings")]
    [Tooltip("Tiempo mínimo en segundos entre ruidos de impacto consecutivas")]
    [SerializeField] private float impactCooldown = 0.25f;

    private bool isTilted = false;
    private float lastImpactTime = -999f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Eliminamos el costoso FindAnyObjectByType. La botella ya no busca activamente.
    }

    private void Update()
    {
        if (!isTilted)
        {
            float currentAngle = Vector3.Angle(transform.up, Vector3.up);
            if (currentAngle > fallAngleThreshold)
            {
                isTilted = true;
                PlaySound(tiltClip, 0.8f);

                // Llamamos directamente al Singleton, pero verificamos que exista primero
                if (NoiseManager.Instance != null)
                    NoiseManager.Instance.AddNoise(tiltNoiseAmount);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastImpactTime < impactCooldown) return;

        float impactForce = collision.relativeVelocity.magnitude;

        // IMPRIMIMOS LA FUERZA REAL EN CONSOLA PARA DEPURAR
        Debug.Log($"[BottleProp] Fuerza de impacto detectada: {impactForce}");

        if (impactForce < minImpactVelocity) return;

        lastImpactTime = Time.time;

        if (impactForce >= hardImpactVelocity)
        {
            Debug.Log("-> Clasificado como: IMPACTO FUERTE");
            PlaySound(hardImpactClip, 1.0f);
            if (NoiseManager.Instance != null) NoiseManager.Instance.AddNoise(hardImpactNoise);
        }
        else
        {
            Debug.Log("-> Clasificado como: IMPACTO SUAVE");
            PlaySound(lightImpactClip, 0.5f);
            if (NoiseManager.Instance != null) NoiseManager.Instance.AddNoise(lightImpactNoise);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class NoiseObstacle : MonoBehaviour
{
    [Header("Impact Settings")]
    [Tooltip("Velocidad mínima del jugador para que el toque inicial haga ruido")]
    [SerializeField] private float minimumPlayerVelocity = 1.5f; // Lo bajé un poco para que el roce horizontal funcione

    [Tooltip("Fuerza mínima con la que el objeto debe chocar contra paredes/suelo para hacer ruido")]
    [SerializeField] private float minimumEnvironmentImpact = 2.0f;

    [SerializeField] private float noiseGenerated = 15f;

    [Header("Audio")]
    [SerializeField] private AudioClip bumpSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Anti-Spam")]
    [SerializeField] private float impactCooldown = 0.5f;

    private AudioSource audioSource;
    private float lastImpactTime = -999f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }

    // 1. Escucha al jugador (El "fantasma" CharacterController)
    public void ReceiveBump(float playerVelocity)
    {
        if (Time.time - lastImpactTime < impactCooldown) return;

        // Si el jugador va muy lento, no hace ruido al rozar
        if (playerVelocity < minimumPlayerVelocity) return;

        Debug.Log($"[NoiseObstacle] El jugador lo tocó. Vel: {playerVelocity}");
        TriggerNoise();
    }

    // 2. Escucha al mundo real (Paredes, suelo, otros objetos)
    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastImpactTime < impactCooldown) return;

        // Ignoramos al jugador aquí por si acaso, ya lo maneja ReceiveBump
        if (collision.gameObject.CompareTag("Player")) return;

        // Validamos la fuerza del impacto contra el entorno
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minimumEnvironmentImpact) return;

        Debug.Log($"[NoiseObstacle] Chocó contra {collision.gameObject.name}. Fuerza: {impactForce}");
        TriggerNoise();
    }

    // Encapsulamos la lógica de hacer ruido para no duplicar código (KISS)
    private void TriggerNoise()
    {
        lastImpactTime = Time.time;

        if (bumpSound != null)
        {
            audioSource.PlayOneShot(bumpSound, volume);
        }

        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.AddNoise(noiseGenerated);
        }
    }
}
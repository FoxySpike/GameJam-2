using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BottleProp : MonoBehaviour
{
    [Header("Dependencies")]
    // Referencia al manager. En un prototipo pequeño FindAnyObjectByType está bien.
    // Si el proyecto crece, consideraríamos inyectarlo para mejorar el rendimiento.
    private NoiseManager noiseManager;
    private AudioSource audioSource;

    [Header("Bump Settings")]
    [Tooltip("Fuerza mínima del choque para que cuente como un golpecito")]
    [SerializeField] private float minBumpVelocity = 0.5f;
    [SerializeField] private float bumpNoiseAmount = 10f;
    [SerializeField] private AudioClip bumpClip;

    [Header("Fall Settings")]
    [Tooltip("Ángulo a partir del cual consideramos que la botella se acostó")]
    [SerializeField] private float fallAngleThreshold = 60f;
    [SerializeField] private float fallNoiseAmount = 25f;
    [SerializeField] private AudioClip fallClip;

    private bool hasFallen = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Buscamos el manager en la escena al iniciar
        noiseManager = FindAnyObjectByType<NoiseManager>();
    }

    private void Update()
    {
        if (hasFallen) return; // Si ya se cayó, dejamos de revisar

        // Calculamos el ángulo entre la punta de la botella y el cielo del mundo
        float currentAngle = Vector3.Angle(transform.up, Vector3.up);

        if (currentAngle > fallAngleThreshold)
        {
            hasFallen = true;
            PlaySound(fallClip, 1f);

            if (noiseManager != null)
                noiseManager.AddNoise(fallNoiseAmount);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasFallen) return; // Si ya está en el suelo, ignoramos golpecitos

        // Medimos qué tan fuerte fue el impacto
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > minBumpVelocity)
        {
            PlaySound(bumpClip, 0.5f);

            if (noiseManager != null)
                noiseManager.AddNoise(bumpNoiseAmount);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
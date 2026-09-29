using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))] // Nos aseguramos de que siempre haya un AudioSource
public class SleeperNPC : MonoBehaviour
{
    [Header("Animation Triggers")]
    [Tooltip("Nombre del Trigger en el Animator para la animación de levantarse")]
    [SerializeField] private string wakeUpTriggerParam = "Despertar";

    [Header("Audio Clips")]
    [Tooltip("Clip de audio en bucle mientras está dormido (ronquidos)")]
    [SerializeField] private AudioClip snoringClip;

    [Tooltip("Clip de audio que suena al despertarse (queja, voz, grito)")]
    [SerializeField] private AudioClip wakeUpClip;

    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Al iniciar la escena, el NPC empieza a roncar automáticamente
        PlaySnoringAudio();
    }

    private void PlaySnoringAudio()
    {
        if (audioSource == null || snoringClip == null) return;

        audioSource.clip = snoringClip;
        audioSource.loop = true; // Hacemos que se repita en bucle
        audioSource.Play();
    }

    public void WakeUp()
    {
        // 1. Ejecutar animación
        if (animator != null)
        {
            animator.SetTrigger(wakeUpTriggerParam);
        }

        // 2. Detener ronquido y reproducir sonido de despertar
        if (audioSource != null)
        {
            audioSource.Stop(); // Cortamos el ronquido de inmediato

            if (wakeUpClip != null)
            {
                audioSource.loop = false; // El grito/frase no debe repetirse
                audioSource.clip = wakeUpClip;
                audioSource.Play();
            }
        }

        Debug.Log("💤 ➡️ 😳 ¡El NPC se ha despertado y ha cambiado de audio!");
    }
}
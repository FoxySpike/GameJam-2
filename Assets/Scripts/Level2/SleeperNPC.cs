using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SleeperNPC : MonoBehaviour
{
    [Header("Animation Triggers")]
    [Tooltip("Nombre del Trigger en el Animator para la animación de levantarse")]
    [SerializeField] private string wakeUpTriggerParam = "WakeUp";

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void WakeUp()
    {
        if (animator != null)
        {
            animator.SetTrigger(wakeUpTriggerParam);
            Debug.Log("💤 ➡️ 😳 ¡El NPC se ha despertado!");
        }
    }
}
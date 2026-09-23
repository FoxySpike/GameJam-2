using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
public sealed class PlayerHitReaction : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerRagdollController ragdollController;
    [SerializeField, Min(0.05f)] private float reactionDuration = 0.55f;
    [SerializeField, Min(0.1f)] private float ragdollDuration = 2f;
    [SerializeField, Min(0f)] private float hitCooldown = 1.25f;

    private Coroutine reaction;
    private float nextAllowedHitTime;

    public bool IsReacting => reaction != null;
    public event Action<Vector3, float> OnHit;

    private void Awake()
    {
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (ragdollController == null) ragdollController = GetComponent<PlayerRagdollController>();
    }

    public bool ReceiveHit(Vector3 direction, float force)
    {
        if (!isActiveAndEnabled || reaction != null || Time.time < nextAllowedHitTime)
            return false;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = -transform.forward;
        direction.Normalize();

        nextAllowedHitTime = Time.time + hitCooldown;
        OnHit?.Invoke(direction, Mathf.Max(0f, force));
        reaction = StartCoroutine(React(direction, Mathf.Max(0f, force)));
        return true;
    }

    private IEnumerator React(Vector3 direction, float force)
    {
        inputReader.SetGameplayBlocked(this, true);

        if (ragdollController != null && ragdollController.BeginRagdoll(direction, force))
        {
            yield return new WaitForSeconds(ragdollDuration);
            ragdollController.EndRagdoll();
            inputReader.SetGameplayBlocked(this, false);
            reaction = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < reactionDuration)
        {
            elapsed += Time.deltaTime;
            float remaining = 1f - Mathf.Clamp01(elapsed / reactionDuration);
            characterController.Move(direction * force * remaining * Time.deltaTime);
            yield return null;
        }

        inputReader.SetGameplayBlocked(this, false);
        reaction = null;
    }

    private void OnDisable()
    {
        if (reaction != null) StopCoroutine(reaction);
        reaction = null;
        if (ragdollController != null) ragdollController.EndRagdoll();
        if (inputReader != null) inputReader.SetGameplayBlocked(this, false);
    }
}

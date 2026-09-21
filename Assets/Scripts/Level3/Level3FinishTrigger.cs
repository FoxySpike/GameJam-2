using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class Level3FinishTrigger : MonoBehaviour
{
    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();

    public bool IsPlayerInside => playerColliders.Count > 0;
    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;

    private void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerHitReaction>() == null) return;
        if (playerColliders.Add(other) && playerColliders.Count == 1)
            OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other)) return;
        if (playerColliders.Count == 0) OnPlayerExited?.Invoke();
    }

    private void OnDisable() => playerColliders.Clear();
}

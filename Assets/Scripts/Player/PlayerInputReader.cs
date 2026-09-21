using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-200), DisallowMultipleComponent]
public sealed class PlayerInputReader : MonoBehaviour
{
    private readonly HashSet<object> blockers = new HashSet<object>();
    private NIS actions;

    public bool GameplayEnabled => isActiveAndEnabled && blockers.Count == 0;
    public Vector2 MoveInput => GameplayEnabled ? actions.Player.Move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 LookInput => GameplayEnabled ? actions.Player.Look.ReadValue<Vector2>() : Vector2.zero;
    public bool Sprint => GameplayEnabled && actions.Player.Sprint.IsPressed();
    public event Action Interact;
    public event Action InputAvailabilityChanged;

    private void Awake()
    {
        actions = new NIS();
        actions.Player.Interact.performed += OnInteract;
    }

    private void OnEnable() => RefreshInput();

    private void OnDisable()
    {
        actions.Disable();
        InputAvailabilityChanged?.Invoke();
    }

    private void OnDestroy()
    {
        actions.Player.Interact.performed -= OnInteract;
        actions.Dispose();
    }

    /// <summary>Owners release only their own lock, so closing a dialogue cannot unlock a blackout.</summary>
    public void SetGameplayBlocked(object owner, bool blocked)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        bool changed = blocked ? blockers.Add(owner) : blockers.Remove(owner);
        if (changed) RefreshInput();
    }

    public bool IsBlockedByOther(object owner) => blockers.Count > (blockers.Contains(owner) ? 1 : 0);

    private void RefreshInput()
    {
        if (GameplayEnabled) actions.Player.Enable();
        else actions.Player.Disable();
        // Notify even if another owner already blocked input: modal UI must yield to the ending.
        InputAvailabilityChanged?.Invoke();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (GameplayEnabled) Interact?.Invoke();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-200), DisallowMultipleComponent]
public sealed class PlayerInputReader : MonoBehaviour
{
    public enum InputContext
    {
        Player,
        Fridge
    }

    private readonly HashSet<object> blockers = new HashSet<object>();
    private NIS actions;

    // Estado de contexto activo (Por defecto inicia en movimiento 3P)
    public InputContext CurrentContext { get; private set; } = InputContext.Player;

    public bool GameplayEnabled => isActiveAndEnabled && blockers.Count == 0;

    // --- ENTRADAS DEL MODO JUGADOR (3P) ---
    public Vector2 MoveInput => (GameplayEnabled && CurrentContext == InputContext.Player) ? actions.Player.Move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 LookInput => (GameplayEnabled && CurrentContext == InputContext.Player) ? actions.Player.Look.ReadValue<Vector2>() : Vector2.zero;
    public bool Sprint => GameplayEnabled && CurrentContext == InputContext.Player && actions.Player.Sprint.IsPressed();

    // --- ENTRADAS DEL MODO NEVERA (1P) ---
    public Vector2 HandMoveInput => (GameplayEnabled && CurrentContext == InputContext.Fridge) ? actions.Fridge.HandMove.ReadValue<Vector2>() : Vector2.zero;
    public bool HoldBreath => GameplayEnabled && CurrentContext == InputContext.Fridge && actions.Fridge.HoldBreath.IsPressed();

    // --- EVENTOS ---
    public event Action Interact;
    public event Action Grab;
    public event Action ExitFridge;
    public event Action InputAvailabilityChanged;

    private void Awake()
    {
        actions = new NIS();

        // Suscripción a eventos del Player Map
        actions.Player.Interact.performed += OnInteractPerformed;

        // Suscripción a eventos del Fridge Map
        actions.Fridge.Grab.performed += OnGrabPerformed;
        actions.Fridge.Exit.performed += OnExitPerformed;
    }

    private void OnEnable() => RefreshInput();

    private void OnDisable()
    {
        actions.Disable();
        InputAvailabilityChanged?.Invoke();
    }

    private void OnDestroy()
    {
        actions.Player.Interact.performed -= OnInteractPerformed;
        actions.Fridge.Grab.performed -= OnGrabPerformed;
        actions.Fridge.Exit.performed -= OnExitPerformed;
        actions.Dispose();
    }

    /// <summary>
    /// Bloquea temporalmente las entradas (útil para menús, diálogos, o cinematográficas).
    /// </summary>
    public void SetGameplayBlocked(object owner, bool blocked)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        bool changed = blocked ? blockers.Add(owner) : blockers.Remove(owner);
        if (changed) RefreshInput();
    }

    public bool IsBlockedByOther(object owner) => blockers.Count > (blockers.Contains(owner) ? 1 : 0);

    /// <summary>
    /// Cambia el contexto de entrada activo entre Player (Caminata 3P) y Fridge (Nevera 1P).
    /// </summary>
    public void SetContext(InputContext newContext)
    {
        if (CurrentContext == newContext) return;

        CurrentContext = newContext;
        RefreshInput();
    }

    private void RefreshInput()
    {
        // Desactivamos ambos mapas para asegurar un punto de partida limpio
        actions.Player.Disable();
        actions.Fridge.Disable();

        if (GameplayEnabled)
        {
            switch (CurrentContext)
            {
                case InputContext.Player:
                    actions.Player.Enable();
                    break;
                case InputContext.Fridge:
                    actions.Fridge.Enable();
                    break;
            }
        }

        InputAvailabilityChanged?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (GameplayEnabled && CurrentContext == InputContext.Player) Interact?.Invoke();
    }

    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        if (GameplayEnabled && CurrentContext == InputContext.Fridge) Grab?.Invoke();
    }

    private void OnExitPerformed(InputAction.CallbackContext context)
    {
        if (GameplayEnabled && CurrentContext == InputContext.Fridge) ExitFridge?.Invoke();
    }
}
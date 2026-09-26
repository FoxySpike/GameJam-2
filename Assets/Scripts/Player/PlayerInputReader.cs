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
        Fridge,
        Grill // 🟢 NUEVO CONTEXTO para el Asador
    }

    private readonly HashSet<object> blockers = new HashSet<object>();
    private NIS actions;

    public InputContext CurrentContext { get; private set; } = InputContext.Player;

    public bool GameplayEnabled => isActiveAndEnabled && blockers.Count == 0;

    // --- ENTRADAS DEL MODO JUGADOR (3P) ---
    public Vector2 MoveInput => (GameplayEnabled && CurrentContext == InputContext.Player) ? actions.Player.Move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 LookInput => (GameplayEnabled && CurrentContext == InputContext.Player) ? actions.Player.Look.ReadValue<Vector2>() : Vector2.zero;
    public bool Sprint => GameplayEnabled && CurrentContext == InputContext.Player && actions.Player.Sprint.IsPressed();

    // --- ENTRADAS DEL MODO NEVERA (1P) ---
    public Vector2 HandMoveInput => (GameplayEnabled && CurrentContext == InputContext.Fridge) ? actions.Fridge.HandMove.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 RigMoveInput => (GameplayEnabled && CurrentContext == InputContext.Fridge) ? actions.Fridge.RigMove.ReadValue<Vector2>() : Vector2.zero;
    public float RigVerticalMoveInput => (GameplayEnabled && CurrentContext == InputContext.Fridge) ? actions.Fridge.RigVerticalMove.ReadValue<float>() : 0f;
    public bool HoldBreath => GameplayEnabled && CurrentContext == InputContext.Fridge && actions.Fridge.HoldBreath.IsPressed();
    public bool IsGrabbing => GameplayEnabled && CurrentContext == InputContext.Fridge && actions.Fridge.Grab.IsPressed();

    // --- ENTRADAS DEL MODO ASADOR (3D) ---
    public Vector2 GrillLookInput => (GameplayEnabled && CurrentContext == InputContext.Grill) ? actions.Grill.Look.ReadValue<Vector2>() : Vector2.zero;
    public float GrillHeatControl => (GameplayEnabled && CurrentContext == InputContext.Grill) ? actions.Grill.HeatControl.ReadValue<float>() : 0f;

    // --- EVENTOS ---
    public event Action Interact;
    public event Action Grab;
    public event Action ExitFridge;
    public event Action ExitGrill;
    public event Action InputAvailabilityChanged;

    private void Awake()
    {
        actions = new NIS();

        // Suscripciones a los eventos de Input System
        actions.Player.Interact.performed += OnInteractPerformed;
        actions.Fridge.Grab.performed += OnGrabPerformed;
        actions.Fridge.Exit.performed += OnExitPerformed;
        actions.Grill.Exit.performed += OnExitGrillPerformed;
    }

    private void OnEnable() => RefreshInput();

    private void OnDisable()
    {
        actions.Disable();
        InputAvailabilityChanged?.Invoke();
    }

    private void OnDestroy()
    {
        // Limpieza de memoria (¡Muy importante!)
        actions.Player.Interact.performed -= OnInteractPerformed;
        actions.Fridge.Grab.performed -= OnGrabPerformed;
        actions.Fridge.Exit.performed -= OnExitPerformed;
        actions.Grill.Exit.performed -= OnExitGrillPerformed;
        actions.Dispose();
    }

    public void SetGameplayBlocked(object owner, bool blocked)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        bool changed = blocked ? blockers.Add(owner) : blockers.Remove(owner);
        if (changed) RefreshInput();
    }

    // 🟢 AQUÍ ESTÁ LA LÍNEA QUE FALTABA Y QUE ROMPÍA TU DIALOGUE UI
    public bool IsBlockedByOther(object owner) => blockers.Count > (blockers.Contains(owner) ? 1 : 0);

    public void SetContext(InputContext newContext)
    {
        if (CurrentContext == newContext) return;
        CurrentContext = newContext;
        RefreshInput();
    }

    private void RefreshInput()
    {
        actions.Player.Disable();
        actions.Fridge.Disable();
        actions.Grill.Disable();

        if (GameplayEnabled)
        {
            switch (CurrentContext)
            {
                case InputContext.Player: actions.Player.Enable(); break;
                case InputContext.Fridge: actions.Fridge.Enable(); break;
                case InputContext.Grill: actions.Grill.Enable(); break;
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

    private void OnExitGrillPerformed(InputAction.CallbackContext context)
    {
        if (GameplayEnabled && CurrentContext == InputContext.Grill) ExitGrill?.Invoke();
    }
}
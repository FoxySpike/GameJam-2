using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private AlcoholSystem alcoholSystem;

    [Serializable]
    private struct MovementProfile
    {
        public AlcoholState state;
        [Min(0f)] public float speedMultiplier;
        [Min(0f)] public float lateralAmplitude;
        [Min(0f)] public float lateralFrequency;

        public MovementProfile(AlcoholState state, float speed, float amplitude, float frequency)
        {
            this.state = state;
            speedMultiplier = speed;
            lateralAmplitude = amplitude;
            lateralFrequency = frequency;
        }
    }

    [Header("State movement profiles")]
    [SerializeField] private MovementProfile[] stateProfiles =
    {
        new MovementProfile(AlcoholState.Sober, 1f, 0f, 0f),
        new MovementProfile(AlcoholState.Tipsy, 1f, 0.04f, 1.4f),
        new MovementProfile(AlcoholState.Drunk, 0.9f, 0.12f, 1.8f),
        new MovementProfile(AlcoholState.Wasted, 0.75f, 0.22f, 2.2f)
    };

    private CharacterController characterController;
    private MovementProfile activeProfile;
    public bool IsMoving { get; private set; }
    public event Action<bool> OnMovingChanged;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();
    }

    private void OnEnable()
    {
        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged += ApplyState;
        inputReader.InputAvailabilityChanged += OnInputAvailabilityChanged;
        ApplyState(alcoholSystem != null ? alcoholSystem.CurrentState : AlcoholState.Sober);
    }

    private void OnDisable()
    {
        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged -= ApplyState;
        inputReader.InputAvailabilityChanged -= OnInputAvailabilityChanged;
        SetMoving(false);
    }

    private void Update()
    {
        Vector2 moveInput = inputReader.MoveInput;
        if (moveInput == Vector2.zero || !characterController.enabled)
        {
            SetMoving(false);
            return;
        }

        float currentSpeed = (inputReader.Sprint ? sprintSpeed : walkSpeed) * activeProfile.speedMultiplier;
        Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y;
        // Instability only affects active locomotion; no passive drift or camera effects.
        movement += transform.right * (Mathf.Sin(Time.time * activeProfile.lateralFrequency)
            * activeProfile.lateralAmplitude * moveInput.magnitude);
        Vector3 before = transform.position;
        characterController.Move(movement * currentSpeed * Time.deltaTime);
        SetMoving((transform.position - before).sqrMagnitude > 0.000001f);
    }

    private void ApplyState(AlcoholState state)
    {
        activeProfile = new MovementProfile(state, 1f, 0f, 0f);
        foreach (MovementProfile profile in stateProfiles)
            if (profile.state == state) { activeProfile = profile; break; }
    }

    private void OnInputAvailabilityChanged()
    {
        if (!inputReader.GameplayEnabled) SetMoving(false);
    }

    private void SetMoving(bool moving)
    {
        if (IsMoving == moving) return;
        IsMoving = moving;
        OnMovingChanged?.Invoke(moving);
    }
}

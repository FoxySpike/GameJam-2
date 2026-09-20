using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private AlcoholSystem alcoholSystem;
    [SerializeField] private Animator animator;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDrunk = Animator.StringToHash("IsDrunk");

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("PlayerAnimationController requires an Animator with a controller.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        movement.OnMovingChanged += SetMoving;
        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged += SetAlcoholState;
        Refresh();
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        movement.OnMovingChanged -= SetMoving;
        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged -= SetAlcoholState;
    }

    private void Refresh()
    {
        SetMoving(movement.IsMoving);
        SetAlcoholState(alcoholSystem != null ? alcoholSystem.CurrentState : AlcoholState.Sober);
    }

    private void SetMoving(bool moving) => animator.SetBool(IsWalking, moving);
    private void SetAlcoholState(AlcoholState state) => animator.SetBool(IsDrunk, state != AlcoholState.Sober);
}

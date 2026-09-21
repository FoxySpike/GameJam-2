using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(PlayerMovementV3))]
public sealed class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerMovementV3 movement;
    [SerializeField] private AlcoholSystem alcoholSystem;
    [SerializeField] private Animator animator;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDrunk = Animator.StringToHash("IsDrunk");

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovementV3>();
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("PlayerAnimationController requiere un Animator con un Controller asignado.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (movement != null) movement.OnMovingChanged += SetMoving;
        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged += SetAlcoholState;

        Refresh();
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        if (movement != null) movement.OnMovingChanged -= SetMoving;
        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged -= SetAlcoholState;
    }

    private void Refresh()
    {
        if (movement != null) SetMoving(movement.IsMoving);
        SetAlcoholState(alcoholSystem != null ? alcoholSystem.CurrentState : NivelBorrachera.Sobrio);
    }

    private void SetMoving(bool moving) => animator.SetBool(IsWalking, moving);

    private void SetAlcoholState(NivelBorrachera state)
    {
        animator.SetBool(IsDrunk, state != NivelBorrachera.Sobrio);
    }
}
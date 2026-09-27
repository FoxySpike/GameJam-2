using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(PlayerMovementV3))]
public sealed class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerMovementV3 movement;
    [SerializeField] private AlcoholSystem alcoholSystem;
    [SerializeField] private Animator animator;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDrunk = Animator.StringToHash("IsDrunk");
    private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");

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
        if (movement != null)
        {
            movement.OnMovingChanged += SetMoving;
            movement.OnSprintingChanged += SetSprinting;
        }

        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged += SetAlcoholState;

        Refresh();
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        if (movement != null)
        {
            movement.OnMovingChanged -= SetMoving;
            movement.OnSprintingChanged -= SetSprinting;
        }

        if (alcoholSystem != null) alcoholSystem.OnAlcoholStateChanged -= SetAlcoholState;
    }

    private void Refresh()
    {
        SetAlcoholState(alcoholSystem != null ? alcoholSystem.CurrentState : NivelBorrachera.Sober);
        SetMoving(movement != null && movement.IsMoving);
        SetSprinting(movement != null && movement.IsSprinting);
    }

    private void SetMoving(bool moving)
    {
        animator.SetBool(IsWalking, moving);

        if (!moving)
            animator.SetBool(IsSprinting, false);
    }

    private void SetSprinting(bool sprinting)
    {
        bool isSober = !animator.GetBool(IsDrunk);
        animator.SetBool(IsSprinting, sprinting && isSober);
    }

    private void SetAlcoholState(NivelBorrachera state)
    {
        bool isDrunk = state != NivelBorrachera.Sober;
        animator.SetBool(IsDrunk, isDrunk);

        if (isDrunk)
            animator.SetBool(IsSprinting, false);
    }
}

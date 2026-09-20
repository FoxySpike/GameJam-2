using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;

    private CharacterController characterController;
    private Animator animator;
    private NIS inputActions;

    private Vector2 moveInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        inputActions = new NIS();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Dispose();
    }

    private void Update()
    {
        Move();
        UpdateAnimation();
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void Move()
    {
        float currentSpeed = walkSpeed;

        if (inputActions.Player.Sprint.IsPressed())
        {
            currentSpeed = sprintSpeed;
        }

        Vector3 movement = transform.right * moveInput.x;
        movement += transform.forward * moveInput.y;

        characterController.Move(movement * currentSpeed * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        bool isWalking = moveInput != Vector2.zero;

        animator.SetBool("IsWalking", isWalking);
    }
}
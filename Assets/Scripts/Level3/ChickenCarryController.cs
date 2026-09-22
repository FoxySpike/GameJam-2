using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public sealed class ChickenCarryController : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private PlayerHitReaction playerHitReaction;
    [SerializeField] private Rigidbody chickenBody;
    [SerializeField] private Collider chickenCollider;
    [SerializeField, Min(0f)] private float dropForce = 5f;
    [SerializeField, Min(0f)] private float upwardForce = 2.5f;
    [SerializeField] private float recoveryHeight = -8f;
    [SerializeField] private string pickupPrompt = "[E] RECOGER POLLO";

    public bool IsHeld { get; private set; }
    public string Prompt => pickupPrompt;
    public event Action OnChickenDropped;
    public event Action OnChickenPickedUp;

    private void Awake()
    {
        if (chickenBody == null) chickenBody = GetComponent<Rigidbody>();
        if (chickenCollider == null) chickenCollider = GetComponent<Collider>();
    }

    public void Configure(Transform targetHoldPoint, PlayerHitReaction hitReaction)
    {
        holdPoint = targetHoldPoint;
        playerHitReaction = hitReaction;
    }

    private void OnEnable()
    {
        if (playerHitReaction != null)
            playerHitReaction.OnHit += OnPlayerHit;
    }

    private void Start() => AttachToPlayer(false);

    private void OnDisable()
    {
        if (playerHitReaction != null)
            playerHitReaction.OnHit -= OnPlayerHit;
    }

    private void Update()
    {
        if (!IsHeld && transform.position.y < recoveryHeight && playerHitReaction != null)
        {
            chickenBody.linearVelocity = Vector3.zero;
            chickenBody.angularVelocity = Vector3.zero;
            transform.position = playerHitReaction.transform.position + playerHitReaction.transform.forward * 1.5f + Vector3.up;
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled && !IsHeld && playerHitReaction != null &&
               interactor.GetComponentInParent<PlayerHitReaction>() == playerHitReaction;
    }

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor)) AttachToPlayer(true);
    }

    private void OnPlayerHit(Vector3 direction, float force)
    {
        if (!IsHeld) return;

        IsHeld = false;
        transform.SetParent(null, true);
        chickenCollider.enabled = true;
        chickenBody.isKinematic = false;
        chickenBody.useGravity = true;
        chickenBody.linearVelocity = Vector3.zero;
        chickenBody.angularVelocity = Vector3.zero;

        Vector3 impulse = direction * Mathf.Min(dropForce, force) + Vector3.up * upwardForce;
        chickenBody.AddForce(impulse, ForceMode.Impulse);
        OnChickenDropped?.Invoke();
    }

    private void AttachToPlayer(bool notify)
    {
        if (holdPoint == null)
        {
            Debug.LogError("ChickenCarryController requires a hold point.", this);
            return;
        }

        IsHeld = true;
        chickenBody.isKinematic = true;
        chickenBody.useGravity = false;
        chickenBody.linearVelocity = Vector3.zero;
        chickenBody.angularVelocity = Vector3.zero;
        chickenCollider.enabled = false;
        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (notify) OnChickenPickedUp?.Invoke();
    }
}

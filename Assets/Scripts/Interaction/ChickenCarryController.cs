using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public sealed class ChickenCarryController : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform carrierRoot;
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
        if (carrierRoot == null && holdPoint != null) carrierRoot = holdPoint.parent;
    }

    public void Configure(Transform targetHoldPoint, Transform targetCarrier)
    {
        holdPoint = targetHoldPoint;
        carrierRoot = targetCarrier;
    }

    private void Start() => AttachToCarrier(false);

    private void Update()
    {
        if (!IsHeld && transform.position.y < recoveryHeight && carrierRoot != null)
        {
            chickenBody.linearVelocity = Vector3.zero;
            chickenBody.angularVelocity = Vector3.zero;
            transform.position = carrierRoot.position + carrierRoot.forward * 1.5f + Vector3.up;
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!isActiveAndEnabled || IsHeld || carrierRoot == null || interactor == null)
            return false;

        Transform interactorTransform = interactor.transform;
        return interactorTransform == carrierRoot || interactorTransform.IsChildOf(carrierRoot);
    }

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor)) AttachToCarrier(true);
    }

    public bool Drop(Vector3 direction, float force)
    {
        if (!IsHeld) return false;

        IsHeld = false;
        transform.SetParent(null, true);
        chickenCollider.enabled = true;
        chickenBody.isKinematic = false;
        chickenBody.useGravity = true;
        chickenBody.linearVelocity = Vector3.zero;
        chickenBody.angularVelocity = Vector3.zero;

        direction.y = 0f;
        Vector3 horizontalImpulse = direction.sqrMagnitude > 0.001f
            ? direction.normalized * Mathf.Min(dropForce, Mathf.Max(0f, force))
            : Vector3.zero;
        Vector3 impulse = horizontalImpulse + Vector3.up * upwardForce;
        chickenBody.AddForce(impulse, ForceMode.Impulse);
        OnChickenDropped?.Invoke();
        return true;
    }

    private void AttachToCarrier(bool notify)
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

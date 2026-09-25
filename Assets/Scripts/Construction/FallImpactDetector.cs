using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class FallImpactDetector : MonoBehaviour
{
    [SerializeField, Min(0f)] private float minFallHeight = 3f;
    [SerializeField, Min(0f)] private float impactForce = 5f;

    public event Action<Vector3, float> OnFallImpact;

    private CharacterController characterController;
    private float peakHeight;
    private bool wasGrounded;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        bool isGrounded = characterController.isGrounded;

        if (!isGrounded)
        {
            // mientras está en el aire, guarda el punto más alto alcanzado
            if (wasGrounded || transform.position.y > peakHeight)
                peakHeight = transform.position.y;
        }
        else if (!wasGrounded)
        {
            // acaba de aterrizar: calcula qué tan profunda fue la caída
            float fallDistance = peakHeight - transform.position.y;

            if (fallDistance >= minFallHeight)
            {
                Vector3 direction = -transform.forward; // o Vector3.zero si no quieres empuje direccional
                OnFallImpact?.Invoke(direction, impactForce);
            }
        }

        wasGrounded = isGrounded;
    }
}
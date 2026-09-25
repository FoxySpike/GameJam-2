using UnityEngine;
using System.Collections.Generic;

public class HandGrabber : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;

    [Tooltip("El GameObject vacío que actúa como la palma de la mano. Usado solo para posición.")]
    [SerializeField] private Transform grabPoint;

    [Tooltip("El Rigidbody del brazo principal (BrazoPivote). NO poner en la mano.")]
    [SerializeField] private Rigidbody armPivotRigidbody; // CORREGIDO: Nombre que refleja la arquitectura real

    [Header("Grab Settings")]
    [SerializeField] private float grabRadius = 0.2f;
    [SerializeField] private LayerMask grabbableLayer;

    private Rigidbody currentlyHeldObject;
    private FixedJoint currentJoint;

    private void Awake()
    {
        if (grabPoint == null)
            Debug.LogError("[HandGrabber] Falta asignar el GrabPoint.");

        if (armPivotRigidbody == null)
            Debug.LogError("[HandGrabber] Falta asignar el Rigidbody del pivote del brazo.");
    }

    private void Update()
    {
        if (inputReader == null || inputReader.CurrentContext != PlayerInputReader.InputContext.Fridge)
            return;

        HandleGrabState();
    }

    private void HandleGrabState()
    {
        bool isHoldingGrabKey = inputReader.IsGrabbing;

        if (isHoldingGrabKey && currentlyHeldObject == null)
        {
            AttemptGrab();
        }
        else if (!isHoldingGrabKey && currentlyHeldObject != null)
        {
            ReleaseObject();
        }
    }

    private void AttemptGrab()
    {
        Collider[] colliders = Physics.OverlapSphere(grabPoint.position, grabRadius, grabbableLayer);

        if (colliders.Length > 0)
        {
            Rigidbody targetRb = colliders[0].GetComponentInParent<Rigidbody>();

            if (targetRb != null)
            {
                GrabObject(targetRb);
            }
        }
    }

    private void GrabObject(Rigidbody target)
    {
        currentlyHeldObject = target;

        // 1. Detenemos cualquier impulso previo del objeto
        currentlyHeldObject.linearVelocity = Vector3.zero;
        currentlyHeldObject.angularVelocity = Vector3.zero;

        // 2. Teleportación INSTANTÁNEA en el fotograma actual (NO usar MovePosition aquí)
        currentlyHeldObject.transform.position = grabPoint.position;
        currentlyHeldObject.transform.rotation = grabPoint.rotation;

        // 3. Al crear el Joint AHORA, detecta ambos objetos en la misma posición y los centra
        currentJoint = armPivotRigidbody.gameObject.AddComponent<FixedJoint>();
        currentJoint.connectedBody = currentlyHeldObject;

        Debug.Log($"[HandGrabber] Agarré y centré con Joint: {currentlyHeldObject.name}");
    }

    private void ReleaseObject()
    {
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        Debug.Log($"[HandGrabber] Solté: {currentlyHeldObject.name}");
        currentlyHeldObject = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class HandGrabber : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;

    [Tooltip("Referencia al controlador del brazo para sincronizar el empuje al agarrar.")]
    [SerializeField] private FridgeHandController handController; // <-- NUEVA DEPENDENCIA

    [Tooltip("El GameObject vacío que actúa como la palma de la mano. Usado solo para posición.")]
    [SerializeField] private Transform grabPoint;

    [Tooltip("El Rigidbody del brazo principal (BrazoPivote). NO poner en la mano.")]
    [SerializeField] private Rigidbody armPivotRigidbody;

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

        if (handController == null)
            Debug.LogWarning("[HandGrabber] No se asignó FridgeHandController. La mano no hará espacio para los objetos.");
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

        // 2. EL TRUCO FÍSICO: Calculamos el vector exacto desde la mano hacia el objeto.
        // Y empujamos el brazo hacia allá ANTES de mover el objeto.
        if (handController != null)
        {
            Vector3 offsetToTarget = currentlyHeldObject.transform.position - grabPoint.position;
            handController.DisplaceHandForGrab(offsetToTarget);
        }

        // 3. Teleportación. Como acabamos de mover la mano al centro del objeto, 
        // este movimiento es ahora de distancia casi cero. No atravesará el cristal.
        currentlyHeldObject.transform.position = grabPoint.position;
        currentlyHeldObject.transform.rotation = grabPoint.rotation;

        // 4. Creamos el Joint
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

        // --- LA SOLUCIÓN ---
        // Le quitamos toda la inercia y temblor heredados del brazo
        // en el instante exacto en que la soltamos.
        if (currentlyHeldObject != null)
        {
            currentlyHeldObject.linearVelocity = Vector3.zero;
            currentlyHeldObject.angularVelocity = Vector3.zero;
        }
        // -------------------

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
using System.Collections;
using UnityEngine;

public class SimpleHandGrabber : MonoBehaviour
{
    [Header("Dependencies")]
    private PlayerInputReader inputReader;
    [SerializeField] private Transform grabPoint;
    [Tooltip("Asigna aquí el BrazoPivote. El script buscará automáticamente todos los colliders hijos (Cilindros, Mano, etc.).")]
    [SerializeField] private Transform playerRoot;

    [Header("Grab Settings")]
    [SerializeField] private float grabRadius = 0.2f;
    [SerializeField] private LayerMask grabbableLayer;

    [Header("Controlador del brazo")]
    [SerializeField] private FridgeHandController handController;

    // Array para almacenar automáticamente todos los colliders del brazo
    private Collider[] playerColliders;

    private Rigidbody heldItemRb;
    private Collider[] heldItemColliders;

    private void Awake()
    {
        if (playerRoot == null)
        {
            Debug.LogWarning("[SimpleHandGrabber] Asigna el PlayerRoot en el Inspector.");
            return;
        }

        // Recopilamos TODOS los colliders que estén dentro de la jerarquía del playerRoot
        playerColliders = playerRoot.GetComponentsInChildren<Collider>();
    }
    private void Start()
    {
        if (PersistentPlayer.Instance != null)
        {
            inputReader = PersistentPlayer.Instance.InputReader;
        }
    }

    private void Update()
    {
        if (inputReader == null || inputReader.CurrentContext != PlayerInputReader.InputContext.Fridge) return;

        if (inputReader.IsGrabbing && heldItemRb == null)
        {
            AttemptGrab();
        }
        else if (!inputReader.IsGrabbing && heldItemRb != null)
        {
            Release();
        }
    }

    private void AttemptGrab()
    {
        Collider[] colliders = Physics.OverlapSphere(grabPoint.position, grabRadius, grabbableLayer);
        if (colliders.Length > 0)
        {
            heldItemRb = colliders[0].GetComponentInParent<Rigidbody>();

            if (heldItemRb != null)
            {
                heldItemRb.isKinematic = true;

                // Extraemos todos los colliders del objeto agarrado
                heldItemColliders = heldItemRb.GetComponentsInChildren<Collider>();

                // Apagamos las colisiones entre el brazo y el objeto
                ToggleCollisions(true);

                heldItemRb.transform.SetParent(grabPoint);
                heldItemRb.transform.localPosition = Vector3.zero;
                heldItemRb.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void Release()
    {
        heldItemRb.transform.SetParent(null);

        // 1. Definimos cuánto vamos a empujar las cosas
        Vector3 pushOffset = Vector3.up * 0.1f;

        // 2. Empujamos la botella
        heldItemRb.transform.position += pushOffset;

        // 3. EMPUJAMOS LA MANO en la misma dirección y distancia
        if (handController != null)
        {
            handController.DisplaceHandForGrab(pushOffset);
        }

        // Reactivamos las físicas
        heldItemRb.isKinematic = false;
        heldItemRb.linearVelocity = Vector3.zero;
        heldItemRb.angularVelocity = Vector3.zero;

        StartCoroutine(RestoreCollisionsAfterDelay(heldItemColliders, playerColliders, 0.25f));

        heldItemRb = null;
        heldItemColliders = null;
    }

    private void ToggleCollisions(bool ignore)
    {
        if (playerColliders == null || heldItemColliders == null) return;

        foreach (Collider itemCol in heldItemColliders)
        {
            foreach (Collider playerCol in playerColliders)
            {
                Physics.IgnoreCollision(itemCol, playerCol, ignore);
            }
        }
    }

    // --- NUEVA CORRUTINA ---
    private IEnumerator RestoreCollisionsAfterDelay(Collider[] itemCols, Collider[] playerCols, float delay)
    {
        // 1. Esperamos una fracción de segundo para que la botella caiga
        yield return new WaitForSeconds(delay);

        // 2. Verificamos que los objetos no hayan sido destruidos en ese cuarto de segundo
        if (itemCols == null || playerCols == null) yield break;

        // 3. Restauramos las colisiones
        foreach (Collider itemCol in itemCols)
        {
            if (itemCol == null) continue; // Por si el objeto se destruyó al chocar

            foreach (Collider playerCol in playerCols)
            {
                if (playerCol == null) continue;

                Physics.IgnoreCollision(itemCol, playerCol, false);
            }
        }
    }
}

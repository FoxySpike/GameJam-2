using UnityEngine;

public class GrabbableItem : MonoBehaviour
{
    [Tooltip("El punto exacto por donde la mano debe agarrar este objeto")]
    public Transform gripPoint;

    // Guardamos tanto la capa como el padre original
    public int originalLayer { get; private set; }
    public Transform OriginalParent { get; private set; } // <--- NUEVO

    private void Awake()
    {
        originalLayer = gameObject.layer;
        OriginalParent = transform.parent; // <--- Guardamos quién era su padre al iniciar

        if (gripPoint == null) gripPoint = transform;
    }
}
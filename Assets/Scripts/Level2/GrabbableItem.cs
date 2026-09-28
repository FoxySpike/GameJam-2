using UnityEngine;

// Se lo pones a la Botella o al Pollo
public class GrabbableItem : MonoBehaviour
{
    [Tooltip("El punto exacto por donde la mano debe agarrar este objeto")]
    public Transform gripPoint;

    // Guardamos la capa original para devolvérsela al soltar
    public int originalLayer { get; private set; }

    private void Awake()
    {
        originalLayer = gameObject.layer;

        // Si se te olvida poner el GripPoint, usamos el centro del objeto por defecto
        if (gripPoint == null) gripPoint = transform;
    }
}
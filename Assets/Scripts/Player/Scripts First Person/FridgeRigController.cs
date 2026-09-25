using UnityEngine;

public class FridgeRigController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Rig Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Tooltip("Límites de movimiento del cuerpo frente a la nevera.")]
    // NOTA MENTAL: Abrí los límites de Y para que pueda subir hasta 0.5 y bajar hasta -0.5
    [SerializeField] private Vector3 minRigBounds = new Vector3(-0.6f, -0.5f, -0.5f);
    [SerializeField] private Vector3 maxRigBounds = new Vector3(0.6f, 0.5f, 0.3f);

    private Vector3 virtualRigPosition;

    private void Awake()
    {
        virtualRigPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        virtualRigPosition = transform.localPosition;
    }

    private void Update()
    {
        if (inputReader == null || inputReader.CurrentContext != PlayerInputReader.InputContext.Fridge)
            return;

        HandleRigMovement();
    }

    private void HandleRigMovement()
    {
        Vector2 xyInput = inputReader.RigMoveInput;        // X = A/D, Y = W/S (En nuestro script, el Y de este input afecta el eje Z)
        float verticalInput = inputReader.RigVerticalMoveInput; // Valor de Q y E

        // EJE X (Izquierda/Derecha)
        virtualRigPosition.x += xyInput.x * moveSpeed * Time.deltaTime;

        // EJE Y (Arriba/Abajo) -> Nuevo input
        virtualRigPosition.y += verticalInput * moveSpeed * Time.deltaTime;

        // EJE Z (Profundidad) -> Recordamos que W/S (el input.y del Vector2) nos mueve en profundidad
        virtualRigPosition.z += xyInput.y * moveSpeed * Time.deltaTime;

        // Aplicamos límites para que no atraviese la nevera ni el suelo
        virtualRigPosition.x = Mathf.Clamp(virtualRigPosition.x, minRigBounds.x, maxRigBounds.x);
        virtualRigPosition.y = Mathf.Clamp(virtualRigPosition.y, minRigBounds.y, maxRigBounds.y);
        virtualRigPosition.z = Mathf.Clamp(virtualRigPosition.z, minRigBounds.z, maxRigBounds.z);

        transform.localPosition = virtualRigPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Transform parentTransform = transform.parent != null ? transform.parent : transform;
        Vector3 center = parentTransform.TransformPoint(new Vector3(
            (minRigBounds.x + maxRigBounds.x) * 0.5f,
            (minRigBounds.y + maxRigBounds.y) * 0.5f,
            (minRigBounds.z + maxRigBounds.z) * 0.5f
        ));
        Vector3 size = new Vector3(
            maxRigBounds.x - minRigBounds.x,
            maxRigBounds.y - minRigBounds.y,
            maxRigBounds.z - minRigBounds.z
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
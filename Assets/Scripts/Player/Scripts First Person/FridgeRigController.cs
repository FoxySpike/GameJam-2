using UnityEngine;

public class FridgeRigController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Rig Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector3 minRigBounds = new Vector3(-1f, -1f, -2f);
    [SerializeField] private Vector3 maxRigBounds = new Vector3(1f, 1f, 0f);

    private void Update()
    {
        if (inputReader == null || inputReader.CurrentContext != PlayerInputReader.InputContext.Fridge)
            return;

        HandleRigMovement();
    }

    private void HandleRigMovement()
    {
        // AQUÍ ES DONDE NECESITAMOS TU DECISIÓN DE DISEÑO
        // Supongamos que agregaste un 'RigMoveInput' a tu InputReader (que podría ser el ratón)
        // Vector2 rigInput = inputReader.RigMoveInput; 

        // Lógica súper básica de traslación del Rig (Cuerpo)
        /*
        Vector3 newPosition = transform.localPosition;
        newPosition.x += rigInput.x * moveSpeed * Time.deltaTime;
        newPosition.z += rigInput.y * moveSpeed * Time.deltaTime; // Asumiendo que Y del input es acercarse (Z)

        // Limitar para que el jugador no atraviese la nevera ni se vaya muy lejos
        newPosition.x = Mathf.Clamp(newPosition.x, minRigBounds.x, maxRigBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minRigBounds.y, maxRigBounds.y);
        newPosition.z = Mathf.Clamp(newPosition.z, minRigBounds.z, maxRigBounds.z);

        transform.localPosition = newPosition;
        */
    }
}
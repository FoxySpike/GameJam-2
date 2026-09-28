using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerPhysicsPusher : MonoBehaviour
{
    [Header("Push Settings")]
    [Tooltip("La velocidad directa que le inyectamos al objeto al empujarlo.")]
    [SerializeField] private float pushPower = 2.0f; // �Volvemos a n�meros peque�os!

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;

        // Obtenemos la direcci�n hacia donde intentamos caminar
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // 1. Calculamos una nueva velocidad. 
        // Dividimos por masa para que las sillas vuelen, pero la nevera no se mueva.
        Vector3 pushVelocity = (pushDirection * pushPower) / rb.mass;

        // 2. �CR�TICO! Rescatamos la velocidad Y actual del objeto.
        // Si no hacemos esto, el objeto dejar�a de caer por la gravedad mientras lo empujas.
        pushVelocity.y = rb.linearVelocity.y;

        // 3. Sobreescribimos la velocidad directamente. �No m�s AddForce!
        rb.linearVelocity = pushVelocity;
    }
}
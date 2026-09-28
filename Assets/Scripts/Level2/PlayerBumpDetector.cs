using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerBumpDetector : MonoBehaviour
{
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Este es un método mágico de Unity exclusivo para CharacterControllers
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Revisamos si con lo que chocamos tiene el componente NoiseObstacle
        if (hit.gameObject.TryGetComponent(out NoiseObstacle obstacle))
        {
            // Le pasamos la velocidad actual a la que se está moviendo el jugador
            float currentSpeed = controller.velocity.magnitude;
            obstacle.ReceiveBump(currentSpeed);
        }
    }
}
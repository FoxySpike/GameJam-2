using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Screenshot : MonoBehaviour
{
    NIS inputActions;

    void Awake()
    {
        // 1. Instanciar la clase autogenerada
        inputActions = new NIS();

        // 2. Suscribir el mismo método a la acción 'Screenshot' en los 3 mapas
        inputActions.Player.Screenshot.performed += TomarCaptura;
        inputActions.Fridge.Screenshot.performed += TomarCaptura;
        inputActions.Grill.Screenshot.performed += TomarCaptura;
    }

    void OnEnable()
    {
        // Habilitar la lectura de inputs
        inputActions.Enable();
    }

    void OnDisable()
    {
        // Deshabilitar inputs
        inputActions.Disable();

        // 3. Desuscribirse para evitar errores de referencia (Memory Leaks)
        inputActions.Player.Screenshot.performed -= TomarCaptura;
        inputActions.Fridge.Screenshot.performed -= TomarCaptura;
        inputActions.Grill.Screenshot.performed -= TomarCaptura;
    }

    // 4. El método que se ejecutará al presionar la tecla
    private void TomarCaptura(InputAction.CallbackContext context)
    {
        ScreenCapture.CaptureScreenshot("screenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
    }
}
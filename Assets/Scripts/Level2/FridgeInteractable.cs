using UnityEngine;

public class FridgeInteractable : MonoBehaviour, IInteractable
{
    [Header("UI & Messaging")]
    [SerializeField] private string promptMessage = "Presiona E para abrir la nevera";

    [Header("Camera Setup")]
    [SerializeField] private Camera fridgeCamera;

    [Header("UI Panels Optional (Opcional)")]
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject fridgeHUD;

    private PlayerInputReader activeInputReader;
    private Camera mainCamera;
    private bool isInMinigame;

    public string Prompt => promptMessage;

    private void Awake()
    {
        fridgeCamera.gameObject.SetActive(false);
    }

    public bool CanInteract(GameObject interactor)
    {
        // No se puede interactuar si ya estamos dentro del minijuego
        return !isInMinigame;
    }

    public void Interact(GameObject interactor)
    {
        if (isInMinigame) return;

        // 1. Obtener el componente del jugador de forma segura
        if (!interactor.TryGetComponent(out activeInputReader))
        {
            Debug.LogWarning($"[FridgeInteractable] El objeto '{interactor.name}' no tiene un componente PlayerInputReader.", this);
            return;
        }

        EnterMinigame();
    }

    private void EnterMinigame()
    {
        isInMinigame = true;

        // 2. Gestionar la transición de cámaras
        mainCamera = Camera.main;
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(true);

        // 3. Gestionar la visibilidad de la interfaz de usuario (UI)
        if (playerHUD != null) playerHUD.SetActive(false);
        if (fridgeHUD != null) fridgeHUD.SetActive(true);

        // 4. Cambiar el contexto de controles en el InputReader
        activeInputReader.SetContext(PlayerInputReader.InputContext.Fridge);

        // 5. Escuchar la orden de salida
        activeInputReader.ExitFridge += ExitMinigame;

        Debug.Log("Entrando a la nevera...");
    }

    private void ExitMinigame()
    {
        if (!isInMinigame) return;

        // 1. Desuscribirse INMEDIATAMENTE para evitar llamadas duplicadas o fugas de memoria
        if (activeInputReader != null)
        {
            activeInputReader.ExitFridge -= ExitMinigame;
            activeInputReader.SetContext(PlayerInputReader.InputContext.Player);
            activeInputReader = null;
        }

        // 2. Restaurar cámaras
        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        // 3. Restaurar UI
        if (fridgeHUD != null) fridgeHUD.SetActive(false);
        if (playerHUD != null) playerHUD.SetActive(true);

        isInMinigame = false;

        Debug.Log("Saliendo de la nevera...");
    }

    private void OnDisable()
    {
        // Limpieza de seguridad: Si el objeto se desactiva mientras jugamos, cancelamos la suscripción
        if (isInMinigame && activeInputReader != null)
        {
            activeInputReader.ExitFridge -= ExitMinigame;
            activeInputReader.SetContext(PlayerInputReader.InputContext.Player);
            isInMinigame = false;
        }
    }
}
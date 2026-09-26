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

    // --- NUEVO: Referencia temporal para simular que el jugador tiene el pollo ---
    [Header("Player Inventory Placeholder")]
    [SerializeField] private GameObject fakePlayerHandItem;
    // -----------------------------------------------------------------------------

    private PlayerInputReader activeInputReader;
    private Camera mainCamera;
    private bool isInMinigame;

    public string Prompt => promptMessage;

    private void Awake()
    {
        fridgeCamera.gameObject.SetActive(false);
        if (fakePlayerHandItem != null) fakePlayerHandItem.SetActive(false);
    }

    public bool CanInteract(GameObject interactor)
    {
        return !isInMinigame;
    }

    public void Interact(GameObject interactor)
    {
        if (isInMinigame) return;

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

        mainCamera = Camera.main;
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(true);

        if (playerHUD != null) playerHUD.SetActive(false);
        if (fridgeHUD != null) fridgeHUD.SetActive(true);

        activeInputReader.SetContext(PlayerInputReader.InputContext.Fridge);

        // OJO: Cambié ExitMinigame a HandleManualExit para diferenciar entre
        // "salir abortando" y "salir ganando".
        activeInputReader.ExitFridge += HandleManualExit;
    }

    // Método intermediario para el input del jugador
    private void HandleManualExit()
    {
        ExitMinigame();
    }

    // --- NUEVO MÉTODO PARA EL TRIGGER ---
    public void OnItemExtracted(GameObject extractedItem)
    {
        // 1. Destruimos el pollo físico que sacaste de la nevera
        Destroy(extractedItem);

        // 2. Buscamos la mano del jugador y le avisamos que active SU pollo visual
        if (activeInputReader != null && activeInputReader.TryGetComponent(out PlayerHand playerHand))
        {
            playerHand.GiveChicken(); // Esto encenderá el modelo que acomodaste en el Paso 1
        }
        else
        {
            Debug.LogWarning("[FridgeInteractable] El jugador no tiene el script PlayerHand.");
        }

        // 3. Salimos de la nevera
        ExitMinigame();
    }
    // ------------------------------------

    private void ExitMinigame()
    {
        if (!isInMinigame) return;

        if (activeInputReader != null)
        {
            activeInputReader.ExitFridge -= HandleManualExit;
            activeInputReader.SetContext(PlayerInputReader.InputContext.Player);
            activeInputReader = null; // Liberar referencia
        }

        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        if (fridgeHUD != null) fridgeHUD.SetActive(false);
        if (playerHUD != null) playerHUD.SetActive(true);

        isInMinigame = false;
    }

    private void OnDisable()
    {
        if (isInMinigame && activeInputReader != null)
        {
            activeInputReader.ExitFridge -= HandleManualExit;
            activeInputReader.SetContext(PlayerInputReader.InputContext.Player);
            isInMinigame = false;
        }
    }
}
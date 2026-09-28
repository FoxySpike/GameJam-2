using UnityEngine;

public class FridgeInteractable : MonoBehaviour, IInteractable
{
    [Header("UI & Messaging")]
    [SerializeField] private string promptMessage = "Presiona F para abrir la nevera";

    [Header("Camera Setup")]
    [SerializeField] private Camera fridgeCamera;

    private PlayerInputReader activeInputReader;
    private Camera mainCamera;
    private bool isInMinigame;

    public string Prompt => promptMessage;

    private void Awake()
    {
        if (fridgeCamera != null)
            fridgeCamera.gameObject.SetActive(false);
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
            Debug.LogWarning($"[FridgeInteractable] El objeto '{interactor.name}' no tiene PlayerInputReader.", this);
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

        // Sigue siendo válido usar el Singleton para la UI global del sistema
        if (PersistentPlayer.Instance != null)
        {
            if (PersistentPlayer.Instance.Player3rdPersonHUD != null)
                PersistentPlayer.Instance.Player3rdPersonHUD.SetActive(false);

            if (PersistentPlayer.Instance.Player1stPersonHUD != null)
                PersistentPlayer.Instance.Player1stPersonHUD.SetActive(true);
        }

        activeInputReader.SetContext(PlayerInputReader.InputContext.Fridge);
        activeInputReader.ExitFridge += HandleManualExit;
    }

    private void HandleManualExit()
    {
        ExitMinigame();
    }

    public void OnItemExtracted(GameObject extractedItem)
    {
        // 1. Validamos que el objeto extraído realmente sea el pollo usando el Tag que le pusiste
        if (!extractedItem.CompareTag("Pollo"))
        {
            Debug.Log($"[FridgeInteractable] Se extrajo un objeto, pero no era el pollo. Era: {extractedItem.name}");
            return;
        }

        // Si llegó hasta aquí, sabemos que es el pollo. Lo destruimos de la escena.
        Destroy(extractedItem);

        if (activeInputReader != null)
        {
            // 2. Usamos GetComponentInChildren. 
            // Esto buscará el PlayerHand en el objeto raíz y en todos sus hijos (donde sea que esté la mano).
            PlayerHand playerHand = activeInputReader.GetComponentInChildren<PlayerHand>();

            if (playerHand != null)
            {
                playerHand.GiveChicken();
            }
            else
            {
                Debug.LogError("[FridgeInteractable] No se encontró el script PlayerHand en el jugador ni en sus hijos.");
            }
        }

        ExitMinigame();
    }

    private void ExitMinigame()
    {
        if (!isInMinigame) return;

        if (activeInputReader != null)
        {
            activeInputReader.ExitFridge -= HandleManualExit;
            activeInputReader.SetContext(PlayerInputReader.InputContext.Player);
            // No hacemos activeInputReader = null; aquí para que OnItemExtracted pueda usarlo.
            // Si quieres limpiar la referencia, hazlo al final de todo.
        }

        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        if (PersistentPlayer.Instance != null)
        {
            if (PersistentPlayer.Instance.Player1stPersonHUD != null)
                PersistentPlayer.Instance.Player1stPersonHUD.SetActive(false);

            if (PersistentPlayer.Instance.Player3rdPersonHUD != null)
                PersistentPlayer.Instance.Player3rdPersonHUD.SetActive(true);
        }

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
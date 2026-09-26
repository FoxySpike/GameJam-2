using UnityEngine;

public class FridgeInteractable : MonoBehaviour, IInteractable
{
    [Header("UI & Messaging")]
    [SerializeField] private string promptMessage = "Presiona E para abrir la nevera";

    [Header("Camera Setup")]
    [SerializeField] private Camera fridgeCamera;

    // ELIMINAMOS los [SerializeField] de la UI. La nevera no sabe dónde están, 
    // se lo preguntará al PersistentPlayer.

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

        // Consultamos al Singleton para apagar el HUD normal y encender el de la nevera
        if (PersistentPlayer.Instance != null)
        {
            if (PersistentPlayer.Instance.Player3rdPersonHUD != null)
                PersistentPlayer.Instance.Player3rdPersonHUD.SetActive(false); // Apagamos 3ra persona

            if (PersistentPlayer.Instance.Player1stPersonHUD != null)
                PersistentPlayer.Instance.Player1stPersonHUD.SetActive(true); // Encendemos 1ra persona (Minijuego)
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
        Destroy(extractedItem);

        if (PersistentPlayer.Instance != null && PersistentPlayer.Instance.PlayerHandComponent != null)
        {
            PersistentPlayer.Instance.PlayerHandComponent.GiveChicken();
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
            activeInputReader = null;
        }

        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        // Consultamos al Singleton para apagar el HUD de la nevera y encender el normal
        if (PersistentPlayer.Instance != null)
        {
            if (PersistentPlayer.Instance.Player1stPersonHUD != null)
                PersistentPlayer.Instance.Player1stPersonHUD.SetActive(false); // Apagamos 1ra persona (Minijuego)

            if (PersistentPlayer.Instance.Player3rdPersonHUD != null)
                PersistentPlayer.Instance.Player3rdPersonHUD.SetActive(true); // Encendemos 3ra persona (Exploración)
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
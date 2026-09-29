using System;
using UnityEngine;

public class FridgeInteractable : MonoBehaviour, IInteractable
{
    [Header("Minigame Dependencies")]
    [Tooltip("Arrastra aquí el brazo pivote que tiene el BreathStaminaSystem")]
    [SerializeField] private BreathStaminaSystem fridgeStaminaSystem;
    [SerializeField] private GameObject fridgeRigRoot; // <--- AÑADE ESTO

    [Header("UI & Messaging")]
    [SerializeField] private string promptMessage = "Presiona F para abrir la nevera";

    [Header("Camera Setup")]
    [SerializeField] private Camera fridgeCamera;

    [Header("Persistent Chicken")]
    [SerializeField] private ChickenCarryController carryChickenPrefab;

    private PlayerInputReader activeInputReader;
    private bool isInMinigame;

    public static event Action OnChickenExtracted;

    public string Prompt => promptMessage;

    private void Awake()
    {
        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(false);
        if (fridgeRigRoot != null) fridgeRigRoot.SetActive(false); // <--- APAGA EL BRAZO AL INICIAR
    }

    public bool CanInteract(GameObject interactor) => !isInMinigame;

    public void Interact(GameObject interactor)
    {
        if (isInMinigame) return;

        // ELIMINA EL TryGetComponent y usa el Singleton como fuente de verdad
        if (PersistentPlayer.Instance != null && PersistentPlayer.Instance.InputReader != null)
        {
            activeInputReader = PersistentPlayer.Instance.InputReader;
        }
        else
        {
            Debug.LogError("No se encontró el InputReader global.");
            return;
        }

        EnterMinigame();
    }

    private void EnterMinigame()
    {
        isInMinigame = true;

        if (fridgeRigRoot != null) fridgeRigRoot.SetActive(true); // <--- ENCIENDE EL BRAZO

        // 1. PRIMERO: Encendemos la cámara (Esto fuerza a que el Awake() 
        // de BreathStaminaSystem se ejecute y CurrentStamina sea 100).
        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(true);

        // 2. SEGUNDO: Ahora sí configuramos la UI. Al leer el sistema, 
        // la estamina ya tendrá el valor correcto.
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetMinigameMode(
                inMinigame: true, 
                showFirstPersonUI: true, 
                showStaminaUI: true, 
                staminaSystem: fridgeStaminaSystem
            );
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
        if (!extractedItem.CompareTag("Pollo"))
        {
            Debug.Log($"[FridgeInteractable] Se extrajo un objeto, pero no era el pollo. Era: {extractedItem.name}");
            return;
        }

        Destroy(extractedItem);

        if (activeInputReader != null && carryChickenPrefab != null)
        {
            ChickenCarryController existingChicken =
                activeInputReader.GetComponentInChildren<ChickenCarryController>(true);

            if (existingChicken == null)
            {
                ChickenCarryController chicken = Instantiate(carryChickenPrefab);
                chicken.Interact(activeInputReader.gameObject);
            }
        }
        else if (carryChickenPrefab == null)
        {
            Debug.LogError("[FridgeInteractable] Falta asignar el prefab persistente del pollo.", this);
        }

        OnChickenExtracted?.Invoke();

        ExitMinigame();
    }

    private void ExitMinigame()
    {
        if (!isInMinigame) return;

        if (activeInputReader != null)
        {
            activeInputReader.ExitFridge -= HandleManualExit;
            activeInputReader.SetContext(PlayerInputReader.InputContext.Player);
        }

        if (fridgeCamera != null) fridgeCamera.gameObject.SetActive(false);
        if (fridgeRigRoot != null) fridgeRigRoot.SetActive(false); // <--- APAGA EL BRAZO AL SALIR

        if (PersistentPlayer.Instance != null)
        {
            // Al salir manualmente, restablece 3ra persona y apaga 1ra persona
            PersistentPlayer.Instance.SetMinigameMode(
                inMinigame: false,
                showFirstPersonUI: false,
                showStaminaUI: false,
                staminaSystem: null
            );
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
using UnityEngine;

public class GrillInteractable : MonoBehaviour, IInteractable
{
    // Mantenemos el estado interno del asador
    private enum GrillState { Empty, Cooking, Has_Raw, Has_Perfect, Has_Burned }

    [Header("Audio")]
    [Tooltip("El sonido placeholder de fritura")]
    [SerializeField] private AudioClip fryingSoundPlaceholder;
    private AudioSource audioSource; // Nuestra referencia al parlante

    [Header("Visuals (Unity Setup)")]
    [SerializeField] private GameObject chickenOnGrillVisual;
    [Tooltip("Arrastra aquí el hijo 'Pollo (1)' que tiene el MeshRenderer")]
    [SerializeField] private Renderer chickenRenderer;

    [Header("Materiales")]
    [SerializeField] private Material rawMaterial;
    [SerializeField] private Material cookedMaterial;
    [SerializeField] private Material burnedMaterial;

    [Header("Prefabs para agarrar (Inventario)")]
    [SerializeField] private ChickenCarryController rawChickenCarryPrefab;
    [SerializeField] private ChickenCarryController cookedChickenCarryPrefab;
    [SerializeField] private ChickenCarryController burnedChickenCarryPrefab;

    [Header("Referencias")]
    [SerializeField] private GrillMinigameController grillMinigame;

    private GrillState currentState = GrillState.Empty;
    private PlayerInputReader currentPlayerInput;

    // NUEVO: Variable para guardar al jugador y poder ocultarlo
    private GameObject currentPlayer;

    public string Prompt
    {
        get
        {
            return currentState switch
            {
                GrillState.Empty => "Press [F] to place raw chicken",
                GrillState.Cooking => "Cooking... Focus!",
                GrillState.Has_Raw => "Press [F] to take RAW chicken (or try again)",
                GrillState.Has_Perfect => "Press [F] to collect PERFECT chicken",
                GrillState.Has_Burned => "Press [F] to throw away BURNED chicken",
                _ => ""
            };
        }
    }

    private void Awake()
    {
        if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(false);
        if (grillMinigame == null) grillMinigame = GetComponent<GrillMinigameController>();

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
    }

    public bool CanInteract(GameObject interactor) => currentState != GrillState.Cooking;

    public void Interact(GameObject interactor)
    {
        if (currentState == GrillState.Empty) TryPlaceChicken(interactor);
        else CollectChicken(interactor);
    }

    private void TryPlaceChicken(GameObject interactor)
    {
        ChickenCarryController chicken = interactor.GetComponentInChildren<ChickenCarryController>(true);

        if (chicken == null || !chicken.IsHeld) return;

        // 1. Guardamos al jugador y lo ocultamos físicamente
        currentPlayer = interactor;
        TogglePlayerVisuals(false);

        Destroy(chicken.gameObject);

        SetChickenMaterial(rawMaterial);
        chickenOnGrillVisual.SetActive(true);

        currentState = GrillState.Cooking;
        currentPlayerInput = PersistentPlayer.Instance.InputReader;
        currentPlayerInput.SetContext(PlayerInputReader.InputContext.Grill);

        // 2. NUEVO: Apagamos la UI y la cámara del jugador usando tu PersistentPlayer
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetMinigameMode(
                inMinigame: true,
                showFirstPersonUI: false, // <-- Lo ponemos en false para que la pantalla esté limpia
                showStaminaUI: false,
                staminaSystem: null
            );
        }

        grillMinigame.OnMinigameEnded += HandleMinigameEnded;
        grillMinigame.StartMinigame(currentPlayerInput);

        if (fryingSoundPlaceholder != null)
        {
            audioSource.clip = fryingSoundPlaceholder;
            audioSource.Play();
        }
    }

    private void HandleMinigameEnded(GrillMinigameController.CookingResult result)
    {
        EndCookingPhase();

        switch (result)
        {
            case GrillMinigameController.CookingResult.Raw:
                SetChickenMaterial(rawMaterial);
                currentState = GrillState.Has_Raw;
                Debug.Log("Pollo crudo. Se puede volver a cocinar o recoger.");
                break;

            case GrillMinigameController.CookingResult.Perfect:
                SetChickenMaterial(cookedMaterial);
                currentState = GrillState.Has_Perfect;
                Debug.Log("Pollo perfecto.");
                break;

            case GrillMinigameController.CookingResult.Burned:
                SetChickenMaterial(burnedMaterial);
                currentState = GrillState.Has_Burned;
                Debug.Log("Pollo quemado.");
                break;
        }
    }

    private void EndCookingPhase()
    {
        grillMinigame.OnMinigameEnded -= HandleMinigameEnded;
        if (currentPlayerInput != null)
        {
            currentPlayerInput.SetContext(PlayerInputReader.InputContext.Player);
            currentPlayerInput = null;
        }

        audioSource.Stop();

        // 3. NUEVO: Restauramos la UI y cámara del jugador al salir
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetMinigameMode(
                inMinigame: false,
                showFirstPersonUI: false,
                showStaminaUI: false,
                staminaSystem: null
            );
        }

        // 4. Volvemos a mostrar el cuerpo del jugador
        TogglePlayerVisuals(true);
        currentPlayer = null;
    }

    private void CollectChicken(GameObject interactor)
    {
        ChickenCarryController prefabToGive = null;

        if (currentState == GrillState.Has_Raw) prefabToGive = rawChickenCarryPrefab;
        else if (currentState == GrillState.Has_Perfect) prefabToGive = cookedChickenCarryPrefab;
        else if (currentState == GrillState.Has_Burned) prefabToGive = burnedChickenCarryPrefab;

        if (prefabToGive != null)
        {
            ChickenCarryController chicken = Instantiate(prefabToGive);
            chicken.Interact(interactor);
        }

        chickenOnGrillVisual.SetActive(false);
        currentState = GrillState.Empty;
    }

    private void SetChickenMaterial(Material material)
    {
        if (chickenRenderer != null && material != null)
            chickenRenderer.sharedMaterial = material;
    }

    // Método de la respuesta anterior para apagar las mallas del jugador
    private void TogglePlayerVisuals(bool isVisible)
    {
        if (currentPlayer != null)
        {
            Renderer[] renderers = currentPlayer.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = isVisible;
            }
        }
    }
}
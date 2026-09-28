using UnityEngine;

public class GrillInteractable : MonoBehaviour, IInteractable
{
    private enum GrillState
    {
        Empty,
        Cooking,
        Finished_Perfect
    }

    [Header("Visuals")]
    [SerializeField] private GameObject chickenOnGrillVisual;
    [SerializeField] private Renderer chickenRenderer;
    [SerializeField] private Material rawChickenMaterial;
    [SerializeField] private Material cookedChickenMaterial;

    [Header("Referencias")]
    [SerializeField] private GrillMinigameController grillMinigame;
    [SerializeField] private ChickenCarryController carryChickenPrefab;

    private GrillState currentState = GrillState.Empty;
    private PlayerInputReader currentPlayerInput;

    public string Prompt
    {
        get
        {
            return currentState switch
            {
                GrillState.Empty => "Press [F] to place the raw chicken on the grill",
                GrillState.Cooking => "Cooking... Focus!",
                GrillState.Finished_Perfect => "Press [F] to collect your cooked chicken",
                _ => ""
            };
        }
    }

    private void Awake()
    {
        SetChickenMaterial(rawChickenMaterial);
        if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(false);
        if (grillMinigame == null) grillMinigame = GetComponent<GrillMinigameController>();
    }

    public bool CanInteract(GameObject interactor)
    {
        return currentState != GrillState.Cooking;
    }

    public void Interact(GameObject interactor)
    {
        switch (currentState)
        {
            case GrillState.Empty:
                TryPlaceChicken(interactor);
                break;
            case GrillState.Finished_Perfect:
                CollectPerfectChicken(interactor);
                break;
        }
    }

    private void TryPlaceChicken(GameObject interactor)
    {
        ChickenCarryController chicken =
            interactor.GetComponentInChildren<ChickenCarryController>(true);

        if (chicken == null || !chicken.IsHeld)
        {
            Debug.Log("No tienes el pollo crudo en la mano.");
            return;
        }

        if (!interactor.TryGetComponent(out currentPlayerInput))
            return;

        Destroy(chicken.gameObject);

        SetChickenMaterial(rawChickenMaterial);

        if (chickenOnGrillVisual != null)
            chickenOnGrillVisual.SetActive(true);

        currentState = GrillState.Cooking;
        currentPlayerInput.SetContext(PlayerInputReader.InputContext.Grill);

        grillMinigame.OnMinigameWon += HandleVictory;
        grillMinigame.OnMinigameLost += HandleDefeat;
        grillMinigame.StartMinigame(currentPlayerInput);
    }

    private void GiveChickenTo(GameObject interactor)
    {
        if (carryChickenPrefab == null)
        {
            Debug.LogError("[GrillInteractable] Falta asignar el prefab persistente del pollo.", this);
            return;
        }

        ChickenCarryController chicken = Instantiate(carryChickenPrefab);
        chicken.Interact(interactor);
    }

    private void HandleVictory()
    {
        EndCookingPhase();
        SetChickenMaterial(cookedChickenMaterial);
        currentState = GrillState.Finished_Perfect;
        Debug.Log("Pollo perfecto listo para recoger.");
    }

    private void HandleDefeat()
    {
        EndCookingPhase();

        // Ocultamos el pollo de la parrilla y la dejamos lista de nuevo
        if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(false);
        currentState = GrillState.Empty;

        // Ejecutamos la lógica/evento de derrota
        OnChickenRuined();
    }

    /// <summary>
    /// Método / Función vacía para implementar más adelante cuando el pollo se arruine.
    /// </summary>
    private void OnChickenRuined()
    {
        // TODO: Agregar partículas de humo negro, sonido de quemado o restar puntuación.
        Debug.Log("[EVENTO DERROTA]: El pollo se arruinó.");
    }

    private void EndCookingPhase()
    {
        grillMinigame.OnMinigameWon -= HandleVictory;
        grillMinigame.OnMinigameLost -= HandleDefeat;

        if (currentPlayerInput != null)
        {
            currentPlayerInput.SetContext(PlayerInputReader.InputContext.Player);
            currentPlayerInput = null;
        }
    }

    private void CollectPerfectChicken(GameObject interactor)
    {
        GiveChickenTo(interactor);

        if (chickenOnGrillVisual != null)
            chickenOnGrillVisual.SetActive(false);

        currentState = GrillState.Empty;
        Debug.Log("Pollo cocinado recogido correctamente.");
    }

    private void SetChickenMaterial(Material material)
    {
        if (chickenRenderer != null && material != null)
            chickenRenderer.sharedMaterial = material;
    }
}

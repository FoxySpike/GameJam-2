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

    [Header("Referencias")]
    [SerializeField] private GrillMinigameController grillMinigame;

    private GrillState currentState = GrillState.Empty;
    private PlayerInputReader currentPlayerInput;

    public string Prompt
    {
        get
        {
            return currentState switch
            {
                GrillState.Empty => "Presiona E para colocar el pollo",
                GrillState.Cooking => "Cocinando... ¡Concéntrate!",
                GrillState.Finished_Perfect => "Presiona E para recoger tu pollo cocinado",
                _ => ""
            };
        }
    }

    private void Awake()
    {
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
        if (interactor.TryGetComponent(out PlayerHand hand) && hand.HasChicken)
        {
            if (interactor.TryGetComponent(out currentPlayerInput))
            {
                hand.ClearHand();
                if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(true);

                currentState = GrillState.Cooking;

                currentPlayerInput.SetContext(PlayerInputReader.InputContext.Grill);

                // Suscripción a eventos del minijuego
                grillMinigame.OnMinigameWon += HandleVictory;
                grillMinigame.OnMinigameLost += HandleDefeat;

                grillMinigame.StartMinigame(currentPlayerInput);
            }
        }
        else
        {
            Debug.Log("No tienes el pollo crudo en la mano.");
        }
    }

    private void HandleVictory()
    {
        EndCookingPhase();
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
        if (interactor.TryGetComponent(out PlayerHand hand))
        {
            // 🟢 ENTREGAMOS EL POLLO COCINADO AL JUGADOR
            hand.GiveChicken();

            if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(false);
            currentState = GrillState.Empty;

            Debug.Log("Pollo cocinado recogido correctamente.");
        }
        else
        {
            Debug.LogWarning("El interactor no tiene el componente PlayerHand.");
        }
    }
}
using UnityEngine;

public class GrillInteractable : MonoBehaviour, IInteractable
{
    private enum GrillState
    {
        Empty,
        Cooking,
        Finished_Perfect,
        Finished_Ruined
    }

    [Header("Visuals")]
    [SerializeField] private GameObject chickenOnGrillVisual;

    [Header("Referencias")]
    [Tooltip("El controlador del minijuego en este mismo objeto")]
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
                GrillState.Finished_Perfect => "Presiona E para recoger tu obra maestra",
                GrillState.Finished_Ruined => "Lo arruinaste. Presiona E para limpiar",
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
        // No se puede interactuar (con la E) mientras se está jugando el minijuego
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
            case GrillState.Finished_Ruined:
                CleanRuinedChicken();
                break;
        }
    }

    private void TryPlaceChicken(GameObject interactor)
    {
        // Verificamos si el jugador tiene el componente de manos y tiene el pollo
        if (interactor.TryGetComponent(out PlayerHand hand) && hand.HasChicken)
        {
            if (interactor.TryGetComponent(out currentPlayerInput))
            {
                hand.ClearHand();
                if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(true);

                currentState = GrillState.Cooking;

                // 1. Cambiamos el input del jugador al modo Grill
                currentPlayerInput.SetContext(PlayerInputReader.InputContext.Grill);

                // 2. Nos suscribimos a los eventos del minijuego
                grillMinigame.OnMinigameWon += HandleVictory;
                grillMinigame.OnMinigameLost += HandleDefeat;

                // 3. Iniciamos pasándole el Input al minijuego
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
        currentState = GrillState.Finished_Ruined;
        Debug.Log("El pollo se arruinó.");
    }

    private void EndCookingPhase()
    {
        // SIEMPRE que te suscribas a un evento (+=), debes desuscribirte (-=) cuando terminas.
        grillMinigame.OnMinigameWon -= HandleVictory;
        grillMinigame.OnMinigameLost -= HandleDefeat;

        // Devolvemos el control normal al jugador
        if (currentPlayerInput != null)
        {
            currentPlayerInput.SetContext(PlayerInputReader.InputContext.Player);
            currentPlayerInput = null; // Limpiamos la referencia
        }
    }

    private void CollectPerfectChicken(GameObject interactor)
    {
        if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(false);
        currentState = GrillState.Empty;
        Debug.Log("Le hemos dado el pollo cocinado al jugador.");
        // Aquí llamarías a hand.GiveItem(polloCocinado) o similar
    }

    private void CleanRuinedChicken()
    {
        if (chickenOnGrillVisual != null) chickenOnGrillVisual.SetActive(false);
        currentState = GrillState.Empty;
        Debug.Log("Limpiaste la parrilla.");
    }
}
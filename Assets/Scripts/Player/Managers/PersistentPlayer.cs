using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance;

    public PlayerInputReader InputReader { get; private set; }

    [Header("Referencias Globales (Asignar en Escena 1)")]
    [Tooltip("Arrastra aquí el objeto 'La Parranda UI'")]
    public GameObject mainPlayerUIRoot;
    public ObjectiveUI PlayerObjective { get; private set; }

    // Ya las tenías aquí, ¡úsalas!
    public GameObject Player3rdPersonHUD;
    public GameObject Player1stPersonHUD;

    [Tooltip("Arrastra aquí la Main Camera hija del jugador")]
    public Camera playerCamera;

    // NUEVO: FALTABA DECLARAR ESTA VARIABLE
    [Header("Referencias de UI de Minijuegos")]
    [Tooltip("Arrastra aquí el script StaminaBarUI que está dentro de tu Player1stPersonHUD")]
    public StaminaBarUI myStaminaBarUI;

    [HideInInspector] public bool isRetryingLevel = false;

    private void Awake()
    {
        PlayerObjective = GetComponentInChildren<ObjectiveUI>(true);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputReader = GetComponent<PlayerInputReader>();

        // Configuramos el estado base AQUÍ.
        // Como Awake se ejecuta ANTES de cualquier Start de la escena, 
        // el jugador ya estará configurado cuando Level2IntroSequence.Start() intente ocultar la UI.
        ResetToDefaultState();
    }

    // Modificamos el método para que acepte un parámetro extra (por defecto falso)
    public void SetMinigameMode(bool inMinigame, bool showFirstPersonUI = false, bool showStaminaUI = false, BreathStaminaSystem staminaSystem = null)
    {
        // 1. Manejo de cámara
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(!inMinigame);

        // 2. Manejo del Interruptor Principal
        // Aseguramos que la raíz de la UI siempre esté encendida si estamos jugando
        // (El GameOverManager o las Cinemáticas usarán SetUIVisibility para apagarlo todo si es necesario)
        if (mainPlayerUIRoot != null && !mainPlayerUIRoot.activeSelf)
        {
            mainPlayerUIRoot.SetActive(true);
        }

        // 3. Alternar los paneles específicos
        if (Player3rdPersonHUD != null)
        {
            // En minijuego, la de 3ra persona se apaga. Si no, se prende.
            Player3rdPersonHUD.SetActive(!inMinigame);
        }

        if (Player1stPersonHUD != null)
        {
            // Solo se prende si estamos en minijuego Y solicitamos la UI de 1ra persona
            Player1stPersonHUD.SetActive(inMinigame && showFirstPersonUI);
        }

        // AHORA SÍ FUNCIONARÁ PORQUE LA VARIABLE EXISTE ARRIBA
        if (myStaminaBarUI != null)
        {
            // Le pasamos el sistema (puede ser null si salimos del minijuego)
            myStaminaBarUI.SetStaminaSystem(staminaSystem);
        }
    }

    // Tu método ResetToDefaultState() YA funciona perfecto con este cambio, 
    // porque llamar a SetMinigameMode(false) automáticamente prenderá el 3rd y apagará el 1st.
    public void ResetToDefaultState()
    {
        SetMinigameMode(false); // Vuelve a la normalidad de forma segura

        if (InputReader != null)
        {
            InputReader.SetContext(PlayerInputReader.InputContext.Player);
            InputReader.SetGameplayBlocked(this, false);
        }
    }

    public void TeleportTo(Transform spawnPoint)
    {
        if (spawnPoint == null) return;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation; // Fija la rotación del padre

        if (controller != null) controller.enabled = true;

        // Le avisamos al control de cámara que se alinee con esta nueva rotación
        PlayerLookV2 lookScript = GetComponentInChildren<PlayerLookV2>();
        if (lookScript != null)
        {
            lookScript.SyncRotation(spawnPoint.rotation.eulerAngles.y);
        }
    }

    // Mantienes tu método antiguo por si lo usas en otro lado
    public void SetUIVisibility(bool isVisible)
    {
        if (mainPlayerUIRoot != null) mainPlayerUIRoot.SetActive(isVisible);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
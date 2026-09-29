using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance;

    public PlayerInputReader InputReader { get; private set; }

    [Header("Referencias Globales (Asignar en Escena 1)")]
    // NUEVO: Referencia al contenedor principal de toda tu UI
    [Tooltip("Arrastra aquí el objeto 'La Parranda UI'")]
    public GameObject mainPlayerUIRoot;
    public ObjectiveUI PlayerObjective { get; private set; }
    public GameObject Player3rdPersonHUD;
    public GameObject Player1stPersonHUD;

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

        ChickenEasterEgg easterEgg = GetComponent<ChickenEasterEgg>();
        if (easterEgg == null) easterEgg = gameObject.AddComponent<ChickenEasterEgg>();
        easterEgg.Initialize();

        if (GetComponent<DrunkenVision>() == null)
            gameObject.AddComponent<DrunkenVision>();

        if (InputReader == null)
            Debug.LogError("[PersistentPlayer] Falta el PlayerInputReader en el jugador!");
    }

    // NUEVO: Método para que otros scripts puedan prender/apagar la UI de forma segura
    public void SetUIVisibility(bool isVisible)
    {
        if (mainPlayerUIRoot != null)
        {
            mainPlayerUIRoot.SetActive(isVisible);
        }
        else
        {
            Debug.LogWarning("[PersistentPlayer] No has asignado el mainPlayerUIRoot en el Inspector.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
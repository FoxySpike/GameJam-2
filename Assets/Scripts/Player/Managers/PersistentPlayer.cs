using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance;

    public PlayerInputReader InputReader { get; private set; }

    [Header("Referencias Globales (Asignar en Escena 1)")]
    [Tooltip("El script de la mano que controla el pollo")]
    public PlayerHand PlayerHandComponent;

    [Tooltip("El contenedor principal de la UI del jugador (1st/3rd person panels)")]
    public GameObject Player3rdPersonHUD;

    // NUEVO: Agregamos el panel de la nevera/1ra persona
    [Tooltip("El HUD para interacciones en primera persona (ej. 1stPersonPanel)")]
    public GameObject Player1stPersonHUD;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputReader = GetComponent<PlayerInputReader>();

        if (InputReader == null)
            Debug.LogError("[PersistentPlayer] ¡Falta el PlayerInputReader en el jugador!");
    }
}
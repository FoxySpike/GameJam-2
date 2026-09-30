using System;
using UnityEngine;
// Añadimos esta librería para usar la carga nativa como Plan B
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Level1FlowController level1Flow;
    [SerializeField] private bool permanentlyLocked;

    [Header("Prueba temporal del easter egg")]
    [Tooltip("Permite ir al nivel 3 al terminar la llamada. Desactivar para restaurar la ruta normal.")]
    [SerializeField] private bool enableLevel3TestShortcut;

    [Header("Requisitos de Inventario")]
    [SerializeField] private bool requireHeldChicken;
    [Tooltip("Estado del pollo requerido para poder cruzar")]
    [SerializeField] private GrillMinigameController.CookingResult requiredChickenStatus = GrillMinigameController.CookingResult.Perfect;

    private bool loading;
    private Collider triggerCollider;

    public event Action OnEntryStarted;

    public string SceneToLoad => sceneToLoad;
    private bool IsTestShortcut => enableLevel3TestShortcut && sceneToLoad == "Nivel 3 - Cruzar la calle";

    public bool IsUnlocked => (!permanentlyLocked || IsTestShortcut) &&
        (gameObject.scene.name != "Nivel 1 - La parranda MVP" ||
         ((sceneToLoad == "Nivel-2-Asadero" || IsTestShortcut) &&
          level1Flow != null && level1Flow.IsReadyForNextLevel));

    // 1. MEJORA: CanEnter ya no falla silenciosamente si falta el SceneLoader
    public bool CanEnter
    {
        get
        {
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            return isActiveAndEnabled && !loading && IsUnlocked && triggerCollider != null &&
                triggerCollider.enabled && triggerCollider.isTrigger;
        }
    }

    private void Awake()
    {
        if (level1Flow != null || gameObject.scene.name != "Nivel 1 - La parranda MVP") return;
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            level1Flow = root.GetComponentInChildren<Level1FlowController>(true);
            if (level1Flow != null) break;
        }
    }

    private void OnTriggerEnter(Collider other) => TryEnter(other);

    private void OnTriggerStay(Collider other)
    {
        if (gameObject.scene.name == "Nivel 1 - La parranda MVP") TryEnter(other);
    }

    private void TryEnter(Collider other)
    {
        if (!CanEnter) return;

        var player = other.GetComponentInParent<PlayerInputReader>();
        if (!other.CompareTag("Player") && player == null) return;

        Debug.Log("🚪 1. El jugador tocó la puerta.");

        if (requireHeldChicken)
        {
            Transform searchRoot = player != null ? player.transform : other.transform;
            ChickenCarryController heldChicken = searchRoot.GetComponentInChildren<ChickenCarryController>(true);

            if (heldChicken == null)
            {
                Debug.LogWarning("❌ 2. NO se encontró el script ChickenCarryController en el jugador.");
                return;
            }

            if (!heldChicken.IsHeld)
            {
                Debug.LogWarning("❌ 2. Tienes el pollo, pero IsHeld es false.");
                return;
            }

            if (heldChicken.ChickenStatus != requiredChickenStatus)
            {
                Debug.LogWarning($"❌ 2. Tienes un pollo {heldChicken.ChickenStatus}, pero la puerta exige {requiredChickenStatus}.");
                return;
            }

            Debug.Log("✅ 2. Pollo perfecto verificado.");
        }

        loading = true;
        OnEntryStarted?.Invoke();

        // 2. MEJORA: El Plan B (Fallback)
        if (SceneLoader.Instance != null)
        {
            Debug.Log($"🚀 3. Cargando escena con SceneLoader: {sceneToLoad}");
            SceneLoader.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            // Si le dimos Play directo al nivel y no hay mánager, usamos la fuerza bruta de Unity para no bloquearnos.
            Debug.LogWarning($"⚠️ 3. SceneLoader.Instance es null (¿Estás probando directo en el nivel?). Usando carga rápida nativa hacia: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
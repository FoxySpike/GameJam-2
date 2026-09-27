using System;
using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Level1FlowController level1Flow;
    [SerializeField] private bool permanentlyLocked;
    [Header("Prueba temporal del easter egg")]
    [Tooltip("Permite ir al nivel 3 al terminar la llamada. Desactivar para restaurar la ruta normal.")]
    [SerializeField] private bool enableLevel3TestShortcut;
    private bool loading;
    private Collider triggerCollider;

    public event Action OnEntryStarted;

    public string SceneToLoad => sceneToLoad;
    private bool IsTestShortcut => enableLevel3TestShortcut &&
        sceneToLoad == "Nivel 3 - Cruzar la calle";

    public bool IsUnlocked => (!permanentlyLocked || IsTestShortcut) &&
        (gameObject.scene.name != "Nivel 1 - La parranda MVP" ||
         ((sceneToLoad == "Nivel-2-Asadero" || IsTestShortcut) &&
          level1Flow != null && level1Flow.IsReadyForNextLevel));

    // Both the marker and the actual transition use the same availability check.
    public bool CanEnter
    {
        get
        {
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            return isActiveAndEnabled && !loading && IsUnlocked && SceneLoader.Instance != null && triggerCollider != null &&
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
    // A player already inside the locked volume need not exit and re-enter after the call.
    private void OnTriggerStay(Collider other)
    {
        if (gameObject.scene.name == "Nivel 1 - La parranda MVP") TryEnter(other);
    }

    private void TryEnter(Collider other)
    {
        if (!CanEnter) return;
        var player = other.GetComponentInParent<PlayerInputReader>();
        if (!other.CompareTag("Player") && player == null) return;
        if (SceneLoader.Instance == null) return;
        loading = true;
        OnEntryStarted?.Invoke();
        SceneLoader.Instance.LoadScene(sceneToLoad);
    }
}

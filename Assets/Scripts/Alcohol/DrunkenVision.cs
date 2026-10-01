using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Controls only the weight of the authored volume; its profile belongs to the designer.</summary>
[DefaultExecutionOrder(100), DisallowMultipleComponent]
public sealed class DrunkenVision : MonoBehaviour
{
    public const string VolumeLayer = "Intoxication";

    [Header("Sistemas")]
    [Tooltip("Arrastra aquí el objeto que tiene el AlcoholSystem (ej. el Player)")]
    [SerializeField] private AlcoholSystem alcohol;
    [SerializeField] private AdulteratedDrinkTracker tracker;

    [Header("Volume editable en la jerarquia")]
    [SerializeField] private Volume intoxicationVolume;
    [Tooltip("Desactivar para ajustar Weight manualmente, incluso durante Play.")]
    [SerializeField] private bool automaticWeight = true;
    [SerializeField, Min(0.05f)] private float transitionSeconds = 1.2f;

    [Header("Peso del efecto por estado")]
    [SerializeField, Range(0f, 1f)] private float soberWeight;
    [SerializeField, Range(0f, 1f)] private float tipsyWeight = 0.3f; // Asegúrate de darles valores > 0
    [SerializeField, Range(0f, 1f)] private float drunkWeight = 0.6f;
    [SerializeField, Range(0f, 1f)] private float wastedWeight = 1f;
    [Tooltip("El easter egg tiene prioridad sobre el estado de alcohol.")]
    [SerializeField, Range(0f, 1f)] private float easterEggWeight = 1f;

    public bool IsManualPreview => !automaticWeight;
    public bool IsIntoxicated => (tracker != null && tracker.IsTriggered) ||
        (alcohol != null && alcohol.CurrentState == NivelBorrachera.Wasted);

    [ContextMenu("Previsualizar mareo durante Play")]
    public void PreviewEffects()
    {
        if (!Application.isPlaying) return;
        automaticWeight = false;
        if (intoxicationVolume != null) intoxicationVolume.weight = 1f;
    }

    [ContextMenu("Volver al estado de alcohol")]
    public void ResumeAutomatic()
    {
        automaticWeight = true;
    }

    private void Awake()
    {
        // 1. Intentamos buscar las referencias si no fueron asignadas en el Inspector
        if (alcohol == null) alcohol = FindObjectOfType<AlcoholSystem>();
        if (tracker == null) tracker = FindObjectOfType<AdulteratedDrinkTracker>();

        if (alcohol == null)
            Debug.LogWarning("DrunkenVision no pudo encontrar un AlcoholSystem en la escena.", this);

        // 2. Buscar el volumen si no está asignado
        if (intoxicationVolume == null)
        {
            int layer = LayerMask.NameToLayer(VolumeLayer);
            foreach (Volume candidate in GetComponentsInChildren<Volume>(true))
            {
                if (candidate.gameObject.layer == layer)
                {
                    intoxicationVolume = candidate;
                    break;
                }
            }
        }

        if (intoxicationVolume == null)
            Debug.LogWarning("Asigna el Global Volume de mareo en DrunkenVision. No se creara uno en runtime.", this);
        else if (automaticWeight)
            intoxicationVolume.weight = GetTargetWeight();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        ConfigurePlayerCameras();
        for (int i = 0; i < SceneManager.sceneCount; i++) ConfigureSceneCameras(SceneManager.GetSceneAt(i));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigurePlayerCameras();
        ConfigureSceneCameras(scene);
    }

    private void ConfigurePlayerCameras()
    {
        foreach (Camera camera in GetComponentsInChildren<Camera>(true))
            IntoxicationCamera.Ensure(camera); // Asumo que este script existe en tu proyecto
    }

    private static void ConfigureSceneCameras(Scene scene)
    {
        if (!scene.isLoaded) return;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
                if (camera.CompareTag("MainCamera")) IntoxicationCamera.Ensure(camera);
    }

    private float GetTargetWeight()
    {
        if (tracker != null && tracker.IsTriggered) return easterEggWeight;
        if (alcohol == null) return soberWeight;

        switch (alcohol.CurrentState)
        {
            case NivelBorrachera.Tipsy: return tipsyWeight;
            case NivelBorrachera.Drunk: return drunkWeight;
            case NivelBorrachera.Wasted: return wastedWeight;
            default: return soberWeight;
        }
    }

    private void LateUpdate()
    {
        if (intoxicationVolume == null || !automaticWeight) return;

        // Transición suave hacia el peso objetivo según el estado actual del alcohol
        intoxicationVolume.weight = Mathf.MoveTowards(
            intoxicationVolume.weight,
            GetTargetWeight(),
            Time.deltaTime / Mathf.Max(0.05f, transitionSeconds)
        );
    }
}
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Animates only lens coordinates in a separate runtime layer, leaving the editable profile intact.</summary>
[DefaultExecutionOrder(200), DisallowMultipleComponent, RequireComponent(typeof(Volume))]
public sealed class DrunkenLensMotion : MonoBehaviour
{
    [Header("Movimiento suave de Lens Distortion")]
    [SerializeField] private bool animateLens = true;
    [SerializeField] private Vector2 centerAmplitude = new Vector2(0.025f, 0.02f);
    [Tooltip("Velocidad de cada eje en ciclos por segundo.")]
    [SerializeField] private Vector2 frequency = new Vector2(0.18f, 0.13f);
    [SerializeField, Range(0f, 0.5f)] private float axisVariation = 0.08f;
    [SerializeField, Min(0.05f)] private float fadeSeconds = 1.2f;

    private Volume source;
    private Volume motionVolume;
    private VolumeProfile motionProfile;
    private LensDistortion animatedLens;
    private DrunkenVision controller;
    private float blend;
    private float phaseX;
    private float phaseY;

    private void Awake()
    {
        source = GetComponent<Volume>();
        controller = GetComponentInParent<DrunkenVision>();
        var motionObject = new GameObject("Lens Motion (runtime)");
        motionObject.transform.SetParent(transform, false);
        motionObject.layer = gameObject.layer;
        motionVolume = motionObject.AddComponent<Volume>();
        motionVolume.isGlobal = true;
        motionVolume.weight = 0f;
        motionProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        motionProfile.name = "Lens motion (runtime only)";
        motionVolume.sharedProfile = motionProfile;
        animatedLens = motionProfile.Add<LensDistortion>(false);
        animatedLens.center.Override(new Vector2(0.5f, 0.5f));
        animatedLens.xMultiplier.Override(1f);
        animatedLens.yMultiplier.Override(1f);
    }

    private void LateUpdate()
    {
        if (source == null || motionVolume == null) return;
        bool requested = animateLens && controller != null &&
            (controller.IsIntoxicated || controller.IsManualPreview);
        blend = Mathf.MoveTowards(blend, requested ? 1f : 0f,
            Time.deltaTime / Mathf.Max(0.05f, fadeSeconds));
        VolumeProfile profile = source.HasInstantiatedProfile() ? source.profile : source.sharedProfile;
        if (!source.isActiveAndEnabled || profile == null ||
            !profile.TryGet(out LensDistortion lens) || !lens.active ||
            !lens.intensity.overrideState || Mathf.Approximately(lens.intensity.value, 0f))
        {
            motionVolume.weight = 0f;
            return;
        }

        phaseX = Mathf.Repeat(phaseX + Time.deltaTime * Mathf.Max(0f, frequency.x) * Mathf.PI * 2f, Mathf.PI * 2f);
        phaseY = Mathf.Repeat(phaseY + Time.deltaTime * Mathf.Max(0f, frequency.y) * Mathf.PI * 2f, Mathf.PI * 2f);
        float x = Mathf.Sin(phaseX);
        float y = Mathf.Sin(phaseY + 1.2f);
        Vector2 center = lens.center.overrideState ? lens.center.value : new Vector2(0.5f, 0.5f);
        animatedLens.center.value = new Vector2(
            Mathf.Clamp01(center.x + x * centerAmplitude.x),
            Mathf.Clamp01(center.y + y * centerAmplitude.y));
        float baseX = lens.xMultiplier.overrideState ? lens.xMultiplier.value : 1f;
        float baseY = lens.yMultiplier.overrideState ? lens.yMultiplier.value : 1f;
        animatedLens.xMultiplier.value = baseX * (1f - axisVariation * (0.5f + 0.5f * x));
        animatedLens.yMultiplier.value = baseY * (1f - axisVariation * (0.5f + 0.5f * y));
        motionVolume.gameObject.layer = gameObject.layer;
        motionVolume.priority = source.priority + 1f;
        motionVolume.weight = source.weight * blend;
    }

    public void PreviewEffects()
    {
        if (controller != null) controller.PreviewEffects();
    }

    public void ResumeAutomatic()
    {
        if (controller != null) controller.ResumeAutomatic();
    }

    private void OnDisable()
    {
        blend = 0f;
        if (motionVolume != null) motionVolume.weight = 0f;
    }

    private void OnDestroy()
    {
        if (motionVolume != null) Destroy(motionVolume.gameObject);
        if (animatedLens != null) Destroy(animatedLens);
        if (motionProfile != null) Destroy(motionProfile);
    }
}

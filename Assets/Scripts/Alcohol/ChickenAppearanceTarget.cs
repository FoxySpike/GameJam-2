using System.Collections.Generic;
using UnityEngine;

/// <summary>Replaces renderers only. Navigation, colliders, dialogue and traffic stay on the host.</summary>
[DisallowMultipleComponent]
public sealed class ChickenAppearanceTarget : MonoBehaviour
{
    [Tooltip("Opcional. Vacio utiliza Resources/EasterEgg/ChickenVisual.")]
    [SerializeField] private GameObject chickenPrefab;
    [Tooltip("Opcional. Vacio busca MeshRenderer/SkinnedMeshRenderer del actor, excluyendo UI.")]
    [SerializeField] private Renderer[] originalRenderers;
    [SerializeField] private Transform visualAnchor;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField, Min(0.01f)] private float sizeMultiplier = 1f;
    [Tooltip("Ajusta el pollo al tamano visual del NPC o vehiculo original.")]
    [SerializeField] private bool fitOriginalBounds = true;

    private GameObject chicken;
    private Animator chickenAnimator;
    private bool[] previousHidden;
    private bool applied;
    private bool started;
    private bool walking;
    private Vector3 previousPosition;

    public static void Ensure(GameObject actor)
    {
        if (actor.GetComponentInParent<PersistentPlayer>() != null ||
            actor.GetComponentInParent<PlayerInputReader>() != null ||
            actor.GetComponentInParent<ChickenCarryController>() != null ||
            actor.GetComponentInParent<ChickenAppearanceTarget>() != null) return;
        actor.AddComponent<ChickenAppearanceTarget>();
    }

    private void OnEnable()
    {
        ChickenEasterEgg.StateChanged += Refresh;
        previousPosition = transform.position;
        if (started) Refresh();
    }

    private void Start()
    {
        // TrafficVehicle creates its visual in Awake; wait until all Awakes have completed.
        started = true;
        Refresh();
    }

    private void Refresh()
    {
        if (!started) return;
        if (!ChickenEasterEgg.IsActive) { Restore(); return; }
        if (applied) return;
        if (chicken == null && !CreateVisual()) return;
        previousHidden = new bool[originalRenderers.Length];
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] == null) continue;
            previousHidden[i] = originalRenderers[i].forceRenderingOff;
            originalRenderers[i].forceRenderingOff = true;
        }
        applied = true;
        chicken.SetActive(true);
        previousPosition = transform.position;
    }

    private bool CreateVisual()
    {
        if (chickenPrefab == null) chickenPrefab = Resources.Load<GameObject>("EasterEgg/ChickenVisual");
        if (chickenPrefab == null)
        {
            Debug.LogError("Falta el prefab visual Resources/EasterEgg/ChickenVisual.", this);
            return false;
        }
        if (originalRenderers == null || originalRenderers.Length == 0)
        {
            var renderers = new List<Renderer>();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                if (renderer.GetComponentInParent<Canvas>() != null) continue;
                if (renderer.GetComponentInParent<ChickenCarryController>() != null) continue;
                renderers.Add(renderer);
            }
            originalRenderers = renderers.ToArray();
        }

        // Props such as the police flashlight must not determine a person's height.
        Renderer[] sizingRenderers = originalRenderers;
        if (GetComponent<TrafficVehicle>() == null)
        {
            var skins = new List<Renderer>();
            foreach (Renderer renderer in originalRenderers)
                if (renderer is SkinnedMeshRenderer) skins.Add(renderer);
            if (skins.Count > 0) sizingRenderers = skins.ToArray();
        }
        bool hasOriginalBounds = TryBounds(sizingRenderers, out Bounds originalBounds);
        Transform anchor = visualAnchor != null ? visualAnchor : transform;
        chicken = Instantiate(chickenPrefab, anchor, false);
        chicken.name = "Chicken appearance";
        chicken.transform.localPosition = Vector3.zero;
        chicken.transform.localRotation = Quaternion.Euler(rotationOffset);
        chicken.transform.localScale = Vector3.one;
        foreach (Transform part in chicken.GetComponentsInChildren<Transform>(true))
            part.gameObject.layer = gameObject.layer;
        chickenAnimator = chicken.GetComponentInChildren<Animator>();
        if (chickenAnimator != null) chickenAnimator.applyRootMotion = false;

        Renderer[] chickenRenderers = chicken.GetComponentsInChildren<Renderer>(true);
        if (fitOriginalBounds && hasOriginalBounds && TryBounds(chickenRenderers, out Bounds birdBounds))
        {
            // Cars use their longest horizontal dimension; people use their height.
            bool vehicle = GetComponent<TrafficVehicle>() != null;
            float targetSize = vehicle ? Mathf.Max(originalBounds.size.x, originalBounds.size.z) : originalBounds.size.y;
            float sourceSize = vehicle ? Mathf.Max(birdBounds.size.x, birdBounds.size.z) : birdBounds.size.y;
            chicken.transform.localScale *= Mathf.Max(0.01f, targetSize) / Mathf.Max(0.01f, sourceSize) * sizeMultiplier;
            if (TryBounds(chickenRenderers, out birdBounds))
            {
                Vector3 originalBase = new Vector3(originalBounds.center.x, originalBounds.min.y, originalBounds.center.z);
                Vector3 birdBase = new Vector3(birdBounds.center.x, birdBounds.min.y, birdBounds.center.z);
                chicken.transform.position += originalBase - birdBase;
            }
        }
        else chicken.transform.localScale *= sizeMultiplier;
        chicken.transform.localPosition += positionOffset;
        return true;
    }

    private static bool TryBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = default;
        bool found = false;
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            if (!found) bounds = renderer.bounds;
            else bounds.Encapsulate(renderer.bounds);
            found = true;
        }
        return found;
    }

    private void LateUpdate()
    {
        if (!applied) return;
        // Force only visibility: original animators may still drive gameplay events.
        foreach (Renderer renderer in originalRenderers)
            if (renderer != null) renderer.forceRenderingOff = true;
        float delta = Time.deltaTime;
        bool moving = delta > 0f && (transform.position - previousPosition).sqrMagnitude > 0.01f * delta * delta;
        previousPosition = transform.position;
        if (chickenAnimator == null || moving == walking) return;
        walking = moving;
        int state = Animator.StringToHash(moving ? "Base Layer.PolloWalk" : "Base Layer.PolloIdle");
        if (chickenAnimator.HasState(0, state)) chickenAnimator.CrossFade(state, 0.15f);
    }

    private void Restore()
    {
        if (!applied) return;
        for (int i = 0; i < originalRenderers.Length; i++)
            if (originalRenderers[i] != null) originalRenderers[i].forceRenderingOff = previousHidden[i];
        if (chicken != null) chicken.SetActive(false);
        applied = false;
    }

    private void OnDisable()
    {
        ChickenEasterEgg.StateChanged -= Refresh;
        Restore();
    }

    private void OnDestroy()
    {
        if (chicken != null) Destroy(chicken);
    }
}

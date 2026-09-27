using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Routes the persistent player's existing drink state to scene visuals.</summary>
[DisallowMultipleComponent]
public sealed class ChickenEasterEgg : MonoBehaviour
{
    public static ChickenEasterEgg Instance { get; private set; }
    public static event Action StateChanged;
    public static bool IsActive => Instance != null && Instance.revealed;

    [Header("Revelacion despues de la llamada")]
    [SerializeField, Min(0.01f)] private float fadeToBlackSeconds = 0.8f;
    [SerializeField, Min(0f)] private float thoughtSeconds = 3.5f;
    [SerializeField, Min(0.01f)] private float fadeFromBlackSeconds = 1.2f;
    [SerializeField, TextArea] private string revealText =
        "Ugh... what did they give me?\nWhat did I drink? Why do I feel like this?";

    private AdulteratedDrinkTracker tracker;
    private bool revealed;
    private bool revealInProgress;
    private CanvasGroup revealOverlay;
    private TMP_Text thoughtText;
    private PlayerInputReader inputReader;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        StateChanged = null;
    }

    private void OnEnable() => Initialize();

    public void Initialize()
    {
        if (Instance == this || !isActiveAndEnabled || PersistentPlayer.Instance == null ||
            PersistentPlayer.Instance.gameObject != gameObject)
            return;
        Instance = this;
        tracker = GetComponent<AdulteratedDrinkTracker>();
        inputReader = GetComponent<PlayerInputReader>();
        if (tracker != null) tracker.OnSpecialIntoxicationTriggered += HandleTriggered;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Notify();
    }

    private void Start()
    {
        if (Instance != this) return;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            RegisterScene(SceneManager.GetSceneAt(i));
        HandleTriggered();
    }

    private static void Notify() => StateChanged?.Invoke();
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterScene(scene);
        HandleTriggered();
    }

    private void HandleTriggered()
    {
        if (tracker == null || !tracker.IsTriggered || revealed || revealInProgress) return;
        // In the party, the phone sequence owns the timing. Other scenes can reveal directly.
        var call = FindFirstObjectByType<WifeCallSequenceController>();
        if (call != null && call.isActiveAndEnabled && !call.IsComplete) return;
        StartCoroutine(RevealAfterCall());
    }

    public IEnumerator RevealAfterCall(TMP_FontAsset font = null)
    {
        if (tracker == null || !tracker.IsTriggered) yield break;
        if (revealInProgress)
        {
            while (revealInProgress) yield return null;
            yield break;
        }
        if (revealed) yield break;
        // Own the coroutine on the persistent player, even when the phone waits for it.
        yield return StartCoroutine(PlayReveal(font));
    }

    private IEnumerator PlayReveal(TMP_FontAsset font)
    {
        revealInProgress = true;
        try
        {
            if (inputReader != null) inputReader.SetGameplayBlocked(this, true);
            CreateRevealOverlay(font);
            revealOverlay.gameObject.SetActive(true);
            revealOverlay.alpha = 0f;
            thoughtText.gameObject.SetActive(false);
            yield return FadeOverlay(1f, fadeToBlackSeconds);

            // Hide the actual model switch behind a completely opaque frame.
            revealed = true;
            Notify();
            thoughtText.text = revealText;
            thoughtText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(thoughtSeconds);
            thoughtText.gameObject.SetActive(false);
            yield return FadeOverlay(0f, fadeFromBlackSeconds);
        }
        finally
        {
            if (revealOverlay != null) revealOverlay.gameObject.SetActive(false);
            if (inputReader != null) inputReader.SetGameplayBlocked(this, false);
            revealInProgress = false;
        }
    }

    private IEnumerator FadeOverlay(float target, float duration)
    {
        float start = revealOverlay.alpha;
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            revealOverlay.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        revealOverlay.alpha = target;
    }

    private void CreateRevealOverlay(TMP_FontAsset font)
    {
        if (revealOverlay != null) return;
        var overlay = new GameObject("Easter Egg Blackout", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        var scaler = overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        revealOverlay = overlay.GetComponent<CanvasGroup>();
        revealOverlay.interactable = false;
        revealOverlay.blocksRaycasts = false;

        var background = new GameObject("Black", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(overlay.transform, false);
        var rect = background.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = background.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        var textObject = new GameObject("Thought", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(overlay.transform, false);
        rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.12f, 0.3f);
        rect.anchorMax = new Vector2(0.88f, 0.7f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        thoughtText = textObject.GetComponent<TextMeshProUGUI>();
        if (font != null) thoughtText.font = font;
        thoughtText.fontSize = 42f;
        thoughtText.enableAutoSizing = true;
        thoughtText.fontSizeMin = 24f;
        thoughtText.fontSizeMax = 42f;
        thoughtText.alignment = TextAlignmentOptions.Center;
        thoughtText.color = Color.white;
        thoughtText.raycastTarget = false;
    }

    private static void RegisterScene(Scene scene)
    {
        if (!scene.isLoaded) return;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // Existing actors need no manual migration. New prefabs can carry the target directly.
            foreach (NPCDialogueController npc in root.GetComponentsInChildren<NPCDialogueController>(true))
                ChickenAppearanceTarget.Ensure(npc.gameObject);
            foreach (PolicePatrol police in root.GetComponentsInChildren<PolicePatrol>(true))
                ChickenAppearanceTarget.Ensure(police.gameObject);
            foreach (TrafficVehicle vehicle in root.GetComponentsInChildren<TrafficVehicle>(true))
                ChickenAppearanceTarget.Ensure(vehicle.gameObject);
            // Includes background people and the wife, without relying on object names or tags.
            foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (!animator.isHuman) continue;
                NavMeshAgent agent = animator.GetComponentInParent<NavMeshAgent>();
                ChickenAppearanceTarget.Ensure(agent != null ? agent.gameObject : animator.gameObject);
            }
        }
    }

    private void OnDisable()
    {
        if (tracker != null) tracker.OnSpecialIntoxicationTriggered -= HandleTriggered;
        StopAllCoroutines();
        if (revealOverlay != null) revealOverlay.gameObject.SetActive(false);
        if (inputReader != null) inputReader.SetGameplayBlocked(this, false);
        revealInProgress = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance != this) return;
        Instance = null;
        Notify();
    }
}

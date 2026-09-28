using System;
using System.Collections;
using TMPro;
using UnityEngine;

public sealed class WifeCallSequenceController : MonoBehaviour
{
    // Kept to hide the old overlay in already-authored scenes during migration.
    [SerializeField] private CanvasGroup blackout;
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private CanvasGroup phoneGroup;
    [SerializeField] private TMP_Text callerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private ObjectiveUI objectiveUI;

    [Header("Call Audio")]
    [SerializeField] private AudioClip ringtoneClip;
    [SerializeField] private AudioClip beepClip;
    [SerializeField] private AudioClip wifeCallClip;

    [SerializeField, Min(0f)] private float slideDuration = 0.35f;
    [SerializeField, Min(0f)] private float incomingCallDuration = 2f;
    [SerializeField, Min(0f)] private float lineDuration = 2.5f;
    [SerializeField] private string callerName = "Wife";
    [SerializeField] private string[] dialogueLines = { "Where are you?", "And where's the chicken? Bring it home!" };
    [SerializeField] private string nextObjective = "GET THE CHICKEN - GO TO THE CHICKEN SHOP";
    private Coroutine sequence;
    private RectTransform phoneRect;
    private Vector2 visiblePosition;
    private bool connected;
    private float connectedAt;
    private AudioSource callAudioSource;

    public bool IsPlaying => sequence != null;
    public bool IsComplete { get; private set; }
    public event Action OnSequenceCompleted;

    public bool IsConfigured => phonePanel != null && callerText != null &&
        dialogueText != null && objectiveUI != null;

    private void Awake()
    {
        if (!IsConfigured)
        {
            Debug.LogError("WifeCallSequenceController requires a phone panel, texts and ObjectiveUI.", this);
            enabled = false;
            return;
        }
        if (blackout != null)
        {
            blackout.alpha = 0f;
            blackout.blocksRaycasts = false;
            blackout.gameObject.SetActive(false);
        }
        phoneRect = phonePanel.GetComponent<RectTransform>();
        callAudioSource = GetComponent<AudioSource>();
        if (callAudioSource == null) callAudioSource = gameObject.AddComponent<AudioSource>();
        callAudioSource.playOnAwake = false;
        callAudioSource.loop = false;
        callAudioSource.spatialBlend = 0f;
        if (phoneRect != null) visiblePosition = phoneRect.anchoredPosition;
        if (phoneGroup != null)
        {
            phoneGroup.interactable = false;
            phoneGroup.blocksRaycasts = false;
        }
        phonePanel.SetActive(false);
    }

    public bool PlaySequence()
    {
        if (!isActiveAndEnabled || !IsConfigured || IsPlaying || IsComplete) return false;
        sequence = StartCoroutine(Play());
        return true;
    }

    private IEnumerator Play()
    {
        if (phoneRect != null)
            phoneRect.anchoredPosition = visiblePosition + Vector2.down * (phoneRect.rect.height + 120f);
        if (phoneGroup != null) phoneGroup.alpha = 0f;
        phonePanel.SetActive(true);
        callerText.text = callerName;
        dialogueText.text = "Incoming call...";
        if (statusText != null) statusText.text = "INCOMING CALL";
        if (timerText != null) timerText.text = string.Empty;

        float ringtoneStartedAt = Time.realtimeSinceStartup;
        PlayCallClip(ringtoneClip);
        yield return Slide(true);
        float remainingRingtoneTime = incomingCallDuration - (Time.realtimeSinceStartup - ringtoneStartedAt);
        if (remainingRingtoneTime > 0f)
            yield return new WaitForSecondsRealtime(remainingRingtoneTime);

        callAudioSource.Stop();
        if (beepClip != null)
        {
            PlayCallClip(beepClip);
            yield return new WaitForSecondsRealtime(beepClip.length);
        }

        connected = true;
        connectedAt = Time.unscaledTime;
        if (statusText != null) statusText.text = "CALL CONNECTED";
        PlayCallClip(wifeCallClip);
        foreach (string line in dialogueLines)
        {
            dialogueText.text = line;
            yield return new WaitForSecondsRealtime(lineDuration);
        }
        callAudioSource.Stop();
        connected = false;
        if (statusText != null) statusText.text = "CALL ENDED";
        dialogueText.text = "You'd better get that chicken.";
        yield return new WaitForSecondsRealtime(1.2f);
        yield return Slide(false);
        phonePanel.SetActive(false);
        if (phoneRect != null) phoneRect.anchoredPosition = visiblePosition;
        // Only adulterated drinks request this reveal. Keep exits locked until it finishes.
        if (ChickenEasterEgg.Instance != null)
            yield return ChickenEasterEgg.Instance.RevealAfterCall(dialogueText.font);
        objectiveUI.SetObjective(nextObjective);
        IsComplete = true;
        sequence = null;
        // The flow unlocks the exit; entering its trigger is what loads the next scene.
        OnSequenceCompleted?.Invoke();
    }

    private void Update()
    {
        if (!connected || timerText == null) return;
        int seconds = Mathf.FloorToInt(Time.unscaledTime - connectedAt);
        timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
    }

    private IEnumerator Slide(bool entering)
    {
        Vector2 hidden = visiblePosition + Vector2.down * (phoneRect != null ? phoneRect.rect.height + 120f : 640f);
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float visibility = entering ? eased : 1f - eased;
            if (phoneRect != null) phoneRect.anchoredPosition = Vector2.Lerp(hidden, visiblePosition, visibility);
            if (phoneGroup != null) phoneGroup.alpha = visibility;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (phoneRect != null) phoneRect.anchoredPosition = entering ? visiblePosition : hidden;
        if (phoneGroup != null) phoneGroup.alpha = entering ? 1f : 0f;
    }

    private void PlayCallClip(AudioClip clip)
    {
        if (clip == null || callAudioSource == null) return;
        callAudioSource.clip = clip;
        callAudioSource.Play();
    }

    private void OnDisable()
    {
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
        connected = false;
        if (callAudioSource != null) callAudioSource.Stop();
        if (phonePanel != null) phonePanel.SetActive(false);
        if (phoneRect != null) phoneRect.anchoredPosition = visiblePosition;
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;

public sealed class WifeCallSequenceController : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackout;
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private TMP_Text callerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField, Min(0f)] private float fadeDuration = 1.2f;
    [SerializeField, Min(0f)] private float incomingCallDuration = 2f;
    [SerializeField, Min(0f)] private float lineDuration = 2.5f;
    [SerializeField] private string callerName = "Esposa";
    [SerializeField] private string[] dialogueLines = { "¿Dónde estás?", "¿Y EL POLLO?" };
    [SerializeField] private string nextObjective = "CONSIGUE EL POLLO";
    private Coroutine sequence;

    public bool IsPlaying => sequence != null;
    public bool IsComplete { get; private set; }
    public event Action OnSequenceCompleted;

    public bool IsConfigured => blackout != null && phonePanel != null && callerText != null &&
        dialogueText != null && objectiveUI != null;

    private void Awake()
    {
        if (!IsConfigured)
        {
            Debug.LogError("WifeCallSequenceController requires blackout, phone panel, texts and ObjectiveUI.", this);
            enabled = false;
            return;
        }
        blackout.alpha = 0f;
        blackout.blocksRaycasts = false;
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
        blackout.gameObject.SetActive(true);
        blackout.blocksRaycasts = true;
        // Real-time timing keeps the narrative running if gameplay time is paused.
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            blackout.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        blackout.alpha = 1f;
        phonePanel.SetActive(true);
        callerText.text = callerName;
        dialogueText.text = "Llamada entrante...";
        yield return new WaitForSecondsRealtime(incomingCallDuration);
        foreach (string line in dialogueLines)
        {
            dialogueText.text = line;
            yield return new WaitForSecondsRealtime(lineDuration);
        }
        objectiveUI.SetObjective(nextObjective);
        IsComplete = true;
        sequence = null;
        // Scene loading is deliberately owned by the future level-transition component.
        OnSequenceCompleted?.Invoke();
    }

    private void OnDisable()
    {
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialogueUI : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text characterText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField, Min(0f)] private float lineDuration = 3f;
    private object owner;
    private bool choosing;
    private Coroutine hideRoutine;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    public bool IsChoosing => choosing;
    public event Action<object, bool> OnChoiceMade;

    private void Awake()
    {
        if (inputReader != null && panel != null && characterText != null &&
            dialogueText != null && acceptButton != null && rejectButton != null)
        {
            panel.SetActive(false);
            return;
        }
        Debug.LogError("DialogueUI requires input, a child panel, both texts and both buttons.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        acceptButton.onClick.AddListener(Accept);
        rejectButton.onClick.AddListener(Reject);
        inputReader.InputAvailabilityChanged += OnInputAvailabilityChanged;
    }

    private void OnDisable()
    {
        if (acceptButton != null) acceptButton.onClick.RemoveListener(Accept);
        if (rejectButton != null) rejectButton.onClick.RemoveListener(Reject);
        if (inputReader != null) inputReader.InputAvailabilityChanged -= OnInputAvailabilityChanged;
        Close();
    }

    public bool ShowOffer(object source, string character, string line)
    {
        if (!isActiveAndEnabled || choosing || !inputReader.GameplayEnabled) return false;
        Show(source, character, line);
        choosing = true;
        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inputReader.SetGameplayBlocked(this, true);
        return true;
    }

    public void ShowLine(object source, string character, string line)
    {
        if (!isActiveAndEnabled || choosing || !inputReader.GameplayEnabled) return;
        Show(source, character, line);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Hide(object source)
    {
        if (ReferenceEquals(owner, source)) Close();
    }

    private void Show(object source, string character, string line)
    {
        Close();
        owner = source;
        characterText.text = character;
        dialogueText.text = line;
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
        panel.SetActive(true);
    }

    private void Accept() => Choose(true);
    private void Reject() => Choose(false);

    private void Choose(bool accepted)
    {
        if (!choosing) return;
        object source = owner;
        Close();
        OnChoiceMade?.Invoke(source, accepted);
    }

    private void OnInputAvailabilityChanged()
    {
        // A narrative lock takes precedence over both an offer and ambient attention text.
        if (inputReader.IsBlockedByOther(this) || !inputReader.isActiveAndEnabled) Close();
    }

    private void Close()
    {
        if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
        owner = null;
        if (panel != null) panel.SetActive(false);
        if (!choosing) return;
        choosing = false;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (inputReader != null) inputReader.SetGameplayBlocked(this, false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(lineDuration);
        hideRoutine = null;
        Close();
    }
}

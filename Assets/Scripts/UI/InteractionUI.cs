using TMPro;
using UnityEngine;

public sealed class InteractionUI : MonoBehaviour
{
    [SerializeField] private PlayerInteraction interaction;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        if (interaction != null && promptText != null) return;
        Debug.LogError("InteractionUI requires PlayerInteraction and prompt text.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        interaction.OnPromptChanged += Show;
        Show(interaction.CurrentPrompt);
    }

    private void OnDisable()
    {
        if (interaction != null) interaction.OnPromptChanged -= Show;
        if (promptText != null) promptText.text = string.Empty;
    }

    private void Show(string prompt) => promptText.text = prompt;
}

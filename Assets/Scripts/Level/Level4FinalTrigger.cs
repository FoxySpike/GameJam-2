using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class Level4FinalTrigger : MonoBehaviour
{
    [SerializeField] private ChickenCarryController chicken;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField, Min(0.1f)] private float messageDuration = 2.5f;
    [SerializeField] private string missingChickenMessage = "Debes llegar con el pollo";

    private Coroutine hideMessageRoutine;
    public bool IsCompleted { get; private set; }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (messagePanel != null) messagePanel.SetActive(false);
    }

    private void Start()
    {
        if (chicken == null)
            chicken = FindAnyObjectByType<ChickenCarryController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsCompleted || !other.CompareTag("Player"))
            return;

        if (chicken != null && chicken.IsHeld)
        {
            CompleteLevel();
            return;
        }

        ShowMissingChickenMessage();
    }

    private void CompleteLevel()
    {
        IsCompleted = true;
        Debug.Log("Nivel 4 completado: el jugador llego con el pollo.", this);
        // Futuro cambio de escena: conectar aqui el cargador de niveles.
    }

    private void ShowMissingChickenMessage()
    {
        if (messageText != null)
            messageText.text = missingChickenMessage;

        if (messagePanel == null)
        {
            Debug.Log(missingChickenMessage, this);
            return;
        }

        messagePanel.SetActive(true);

        if (hideMessageRoutine != null)
            StopCoroutine(hideMessageRoutine);

        hideMessageRoutine = StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messagePanel != null) messagePanel.SetActive(false);
        hideMessageRoutine = null;
    }
}

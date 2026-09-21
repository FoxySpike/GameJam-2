using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(NPCDialogueController))]
public sealed class NPCDrinkOffer : MonoBehaviour, IInteractable
{
    [SerializeField] private DrinkData offeredDrink;
    [SerializeField] private NPCDialogueController dialogue;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private bool singleOffer = true;
    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
    private DrinkingSystem nearbyPlayer;
    private bool offerUsed;
    public string Prompt => "[E] Hablar";

    private void Awake()
    {
        if (dialogue == null) dialogue = GetComponent<NPCDialogueController>();
        if (offeredDrink != null && dialogueUI != null) return;
        Debug.LogError("NPCDrinkOffer requires DrinkData and the scene DialogueUI.", this);
        enabled = false;
    }

    private void OnEnable() => dialogueUI.OnChoiceMade += OnChoiceMade;

    private void OnDisable()
    {
        if (dialogueUI != null)
        {
            dialogueUI.OnChoiceMade -= OnChoiceMade;
            dialogueUI.Hide(this);
        }
        playerColliders.Clear();
        nearbyPlayer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;
        DrinkingSystem player = other.GetComponentInParent<DrinkingSystem>();
        if (player == null || (nearbyPlayer != null && nearbyPlayer != player)) return;
        nearbyPlayer = player;
        if (!playerColliders.Add(other) || playerColliders.Count != 1 || offerUsed) return;
        dialogueUI.ShowLine(this, dialogue.CharacterName, dialogue.Attention);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other) || playerColliders.Count != 0) return;
        dialogueUI.Hide(this);
        nearbyPlayer = null;
    }

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && !offerUsed && nearbyPlayer != null &&
        nearbyPlayer.gameObject == interactor && !dialogueUI.IsChoosing;

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor))
            dialogueUI.ShowOffer(this, dialogue.CharacterName, dialogue.Offer);
    }

    private void OnChoiceMade(object source, bool accepted)
    {
        if (!ReferenceEquals(source, this) || nearbyPlayer == null || offerUsed) return;
        if (accepted)
        {
            offerUsed = singleOffer;
            if (!nearbyPlayer.ConsumeDrink(offeredDrink)) { offerUsed = false; return; }

            NPCApproachPlayer approach = GetComponent<NPCApproachPlayer>();
            if (approach != null)
                approach.StopFollowing();
        }
        dialogueUI.ShowLine(this, dialogue.CharacterName, accepted ? dialogue.Accepted : dialogue.Rejected);
    }
}

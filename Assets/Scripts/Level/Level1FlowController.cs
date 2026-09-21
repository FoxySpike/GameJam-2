using System;
using UnityEngine;

public sealed class Level1FlowController : MonoBehaviour
{
    [SerializeField] private AlcoholSystem alcoholSystem;
    [SerializeField] private AdulteratedDrinkTracker adulteratedTracker;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private WifeCallSequenceController wifeCallSequence;
    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private string initialObjective = "EMBORRÁCHATE";
    private bool initialized;

    public bool HasFinishedParty { get; private set; }
    public bool IsReadyForNextLevel { get; private set; }
    public event Action OnReadyForNextLevel;

    private void Awake()
    {
        if (alcoholSystem != null && adulteratedTracker != null && inputReader != null &&
            wifeCallSequence != null && objectiveUI != null) return;
        Debug.LogError("Level1FlowController requires alcohol, tracker, input, call sequence and objective UI.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        alcoholSystem.OnMaxAlcoholReached += FinishParty;
        adulteratedTracker.OnSpecialIntoxicationTriggered += FinishParty;
        wifeCallSequence.OnSequenceCompleted += CompleteLevel;
        if (initialized) SynchronizeFlow();
    }

    private void Start()
    {
        initialized = true;
        if (!HasFinishedParty) objectiveUI.SetObjective(initialObjective);
        SynchronizeFlow();
    }

    private void SynchronizeFlow()
    {
        if (HasFinishedParty)
        {
            inputReader.SetGameplayBlocked(this, true);
            if (wifeCallSequence.IsComplete) CompleteLevel();
            else if (!wifeCallSequence.IsPlaying) wifeCallSequence.PlaySequence();
            return;
        }
        // Initialization never depends on which listener subscribed first.
        if (alcoholSystem.IsAtMaximum || adulteratedTracker.IsTriggered) FinishParty();
    }

    private void OnDisable()
    {
        if (alcoholSystem != null) alcoholSystem.OnMaxAlcoholReached -= FinishParty;
        if (adulteratedTracker != null) adulteratedTracker.OnSpecialIntoxicationTriggered -= FinishParty;
        if (wifeCallSequence != null) wifeCallSequence.OnSequenceCompleted -= CompleteLevel;
        if (inputReader != null) inputReader.SetGameplayBlocked(this, false);
    }

    private void FinishParty()
    {
        if (HasFinishedParty) return;
        if (!wifeCallSequence.IsConfigured || !wifeCallSequence.isActiveAndEnabled)
        {
            Debug.LogError("Cannot finish La Parranda: configure and enable the wife call sequence.", this);
            return;
        }
        // Set the guard before either route can dispatch another ending event.
        HasFinishedParty = true;
        inputReader.SetGameplayBlocked(this, true);
        wifeCallSequence.PlaySequence();
    }

    private void CompleteLevel()
    {
        if (IsReadyForNextLevel) return;
        IsReadyForNextLevel = true;
        OnReadyForNextLevel?.Invoke();
    }
}

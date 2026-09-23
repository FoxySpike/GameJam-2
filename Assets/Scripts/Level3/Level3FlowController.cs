using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Level3FlowController : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private ChickenCarryController chicken;
    [SerializeField] private Level3FinishTrigger finishTrigger;
    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private string crossStreetObjective = "CRUZA LA CALLE";
    [SerializeField] private string recoverChickenObjective = "RECUPERA EL POLLO";
    [SerializeField] private string completedObjective = "NIVEL COMPLETADO";

    public bool IsCompleted { get; private set; }
    public event Action OnLevelCompleted;

    public void Configure(
        PlayerInputReader input,
        ChickenCarryController carriedChicken,
        Level3FinishTrigger finish,
        ObjectiveUI objective)
    {
        inputReader = input;
        chicken = carriedChicken;
        finishTrigger = finish;
        objectiveUI = objective;
    }

    private void OnEnable()
    {
        if (chicken != null)
        {
            chicken.OnChickenDropped += ShowRecoverChicken;
            chicken.OnChickenPickedUp += OnChickenPickedUp;
        }

        if (finishTrigger != null)
            finishTrigger.OnPlayerEntered += EvaluateFinish;
    }

    private void Start()
    {
        if (!IsConfigured())
        {
            enabled = false;
            return;
        }

        objectiveUI.SetObjective(crossStreetObjective);
    }

    private void OnDisable()
    {
        if (chicken != null)
        {
            chicken.OnChickenDropped -= ShowRecoverChicken;
            chicken.OnChickenPickedUp -= OnChickenPickedUp;
        }

        if (finishTrigger != null)
            finishTrigger.OnPlayerEntered -= EvaluateFinish;

        if (inputReader != null)
            inputReader.SetGameplayBlocked(this, false);
    }

    private bool IsConfigured()
    {
        bool configured = inputReader != null && chicken != null &&
                          finishTrigger != null && objectiveUI != null;
        if (!configured) Debug.LogError("Level3FlowController has missing scene references.", this);
        return configured;
    }

    private void ShowRecoverChicken()
    {
        if (!IsCompleted) objectiveUI.SetObjective(recoverChickenObjective);
    }

    private void OnChickenPickedUp()
    {
        if (IsCompleted) return;
        if (finishTrigger.IsPlayerInside) CompleteLevel();
        else objectiveUI.SetObjective(crossStreetObjective);
    }

    private void EvaluateFinish()
    {
        if (IsCompleted) return;
        if (chicken.IsHeld) CompleteLevel();
        else objectiveUI.SetObjective(recoverChickenObjective);
    }

    private void CompleteLevel()
    {
        if (IsCompleted) return;
        IsCompleted = true;
        inputReader.SetGameplayBlocked(this, true);
        objectiveUI.SetObjective(completedObjective);
        OnLevelCompleted?.Invoke();
        Debug.Log("Street crossing completed.", this);
    }
}

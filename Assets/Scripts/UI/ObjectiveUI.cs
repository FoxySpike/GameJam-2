using TMPro;
using UnityEngine;

public sealed class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    public string CurrentObjective { get; private set; } = string.Empty;

    private void Awake()
    {
        if (objectiveText == null) Debug.LogError("ObjectiveUI requires objective text.", this);
    }

    public void SetObjective(string objective)
    {
        CurrentObjective = objective;
        if (objectiveText != null) objectiveText.text = objective;
    }
}

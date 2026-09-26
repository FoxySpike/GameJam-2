using UnityEngine;

/// <summary>Shows a replaceable cube only while its own entrance can be used.</summary>
public sealed class LevelExitBeacon : MonoBehaviour
{
    [SerializeField] private SceneTrigger exit;
    [SerializeField] private GameObject visuals;

    private void Awake()
    {
        if (visuals != null) visuals.SetActive(false);
    }

    private void OnEnable()
    {
        if (exit != null) exit.OnEntryStarted += Hide;
        Refresh();
    }

    private void OnDisable()
    {
        if (exit != null) exit.OnEntryStarted -= Hide;
        Hide();
    }

    private void Hide()
    {
        if (visuals != null) visuals.SetActive(false);
    }

    private void LateUpdate() => Refresh();

    private void Refresh()
    {
        bool visible = exit != null && exit.CanEnter;
        if (visuals != null && visuals.activeSelf != visible) visuals.SetActive(visible);
    }
}

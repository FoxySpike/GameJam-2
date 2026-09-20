using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AlcoholHUD : MonoBehaviour
{
    [SerializeField] private AlcoholSystem alcoholSystem;
    [SerializeField] private AdulteratedDrinkTracker adulteratedTracker;
    [SerializeField] private Slider alcoholBar;
    [SerializeField] private TMP_Text stateText;

    private void Awake()
    {
        if (alcoholSystem != null && alcoholBar != null && stateText != null) return;
        Debug.LogError("AlcoholHUD requires AlcoholSystem, Slider and state text.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        alcoholSystem.OnAlcoholChanged += ShowValue;
        alcoholSystem.OnAlcoholStateChanged += ShowState;
        if (adulteratedTracker != null) adulteratedTracker.OnSpecialIntoxicationTriggered += ShowSpecial;
        ShowValue(alcoholSystem.CurrentAlcohol);
        ShowState(alcoholSystem.CurrentState);
    }

    private void OnDisable()
    {
        if (alcoholSystem != null)
        {
            alcoholSystem.OnAlcoholChanged -= ShowValue;
            alcoholSystem.OnAlcoholStateChanged -= ShowState;
        }
        if (adulteratedTracker != null) adulteratedTracker.OnSpecialIntoxicationTriggered -= ShowSpecial;
    }

    private void ShowValue(float value)
    {
        alcoholBar.minValue = 0f;
        alcoholBar.maxValue = alcoholSystem.MaxAlcohol;
        alcoholBar.SetValueWithoutNotify(value);
    }

    private void ShowState(AlcoholState state)
    {
        if (adulteratedTracker != null && adulteratedTracker.IsTriggered) { ShowSpecial(); return; }
        switch (state)
        {
            case AlcoholState.Sober: stateText.text = "SOBRIO"; break;
            case AlcoholState.Tipsy: stateText.text = "PRENDIDO"; break;
            case AlcoholState.Drunk: stateText.text = "BORRACHO"; break;
            case AlcoholState.Wasted: stateText.text = "VUELTO MIERDA"; break;
        }
    }

    private void ShowSpecial() => stateText.text = "???";
}

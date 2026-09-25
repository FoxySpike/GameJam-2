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
        Debug.LogError("AlcoholHUD requiere AlcoholSystem, Slider y texto de estado.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        alcoholSystem.OnAlcoholChanged += ShowValue;
        alcoholSystem.OnAlcoholStateChanged += ShowState; // Firma corregida

        if (adulteratedTracker != null)
            adulteratedTracker.OnSpecialIntoxicationTriggered += ShowSpecial;

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

        if (adulteratedTracker != null)
            adulteratedTracker.OnSpecialIntoxicationTriggered -= ShowSpecial;
    }

    private void ShowValue(float value)
    {
        alcoholBar.minValue = 0f;
        alcoholBar.maxValue = alcoholSystem.MaxAlcohol;
        alcoholBar.SetValueWithoutNotify(value);
    }

    // CORREGIDO: Ahora recibe NivelBorrachera
    private void ShowState(NivelBorrachera state)
    {
        if (adulteratedTracker != null && adulteratedTracker.IsTriggered)
        {
            ShowSpecial();
            return;
        }

        switch (state)
        {
            case NivelBorrachera.Sober:
                stateText.text = "Sober";
                break;
            case NivelBorrachera.Tipsy:
                stateText.text = "Tipsy";
                break;
            case NivelBorrachera.Drunk:
                stateText.text = "Drunk";
                break;
            case NivelBorrachera.Wasted:
                stateText.text = "VUELTO MIERDA"; // Aqu� podemos incluir el espacio c�modamente
                break;
        }
    }

    private void ShowSpecial() => stateText.text = "???";
}
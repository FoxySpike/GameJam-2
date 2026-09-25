using UnityEngine;

[DisallowMultipleComponent]
public sealed class DrinkInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DrinkData drinkData;
    [SerializeField] private bool singleUse = true;
    private bool consumed;
    public string Prompt => "[F] Beber";

    private void Awake()
    {
        if (drinkData == null) Debug.LogError("DrinkInteractable requires DrinkData.", this);
    }

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && !consumed && drinkData != null &&
        interactor.TryGetComponent(out DrinkingSystem drinking) && drinking.isActiveAndEnabled;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        // Mark before dispatching events to prevent reentrant consumption of the same bottle.
        consumed = singleUse;
        if (!interactor.GetComponent<DrinkingSystem>().ConsumeDrink(drinkData))
        {
            consumed = false;
            return;
        }
        if (singleUse) gameObject.SetActive(false);
    }
}

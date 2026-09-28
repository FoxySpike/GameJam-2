using UnityEngine;

[DisallowMultipleComponent]
public sealed class DrinkInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DrinkData drinkData;
    [SerializeField] private bool singleUse = true;
    [SerializeField] private string interactionPrompt = "[F] Drink";
    [SerializeField] private AudioSource drinkAudioSource;
    private bool consumed;
    public string Prompt => interactionPrompt;

    private void Awake()
    {
        if (drinkAudioSource == null)
            drinkAudioSource = GetComponentInChildren<AudioSource>(true);
        if (drinkData == null) Debug.LogError("DrinkInteractable requires DrinkData.", this);
    }

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && !consumed && drinkData != null &&
        interactor.TryGetComponent(out DrinkingSystem drinking) && drinking.isActiveAndEnabled;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        DrinkingSystem drinking = interactor.GetComponent<DrinkingSystem>();
        // Mark before dispatching events to prevent reentrant consumption of the same bottle.
        consumed = singleUse;
        if (!drinking.ConsumeDrink(drinkData))
        {
            consumed = false;
            return;
        }
        if (drinkAudioSource != null && drinkAudioSource.clip != null)
            drinking.PlayDrinkSound(drinkAudioSource);
        else
            Debug.LogWarning("La bebida no tiene un AudioSource con un AudioClip asignado.", this);
        if (singleUse) gameObject.SetActive(false);
    }
}

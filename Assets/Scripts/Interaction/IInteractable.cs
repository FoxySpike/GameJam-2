using UnityEngine;

/// <summary>Small contract for world interactions; the caller never needs the concrete object type.</summary>
public interface IInteractable
{
    string Prompt { get; }
    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}

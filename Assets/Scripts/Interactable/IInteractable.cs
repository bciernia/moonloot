using UnityEngine;

public interface IInteractable
{
    void Interact();
    string GetInteractionText();
}

public enum InteractableType
{
    NPC,
    Item,
    Door
}
using UnityEngine;

public interface IInteractable
{
    string GetInteractionPrompt();
    void OnInteract(GameObject interactor);
}

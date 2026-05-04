using UnityEngine;

// Replace InteractableObject with this component on beer bottle GameObjects.
// Set the inherited "Interaction Prompt" field in the Inspector (e.g. "Left Click to Drink Beer").
public class DrinkableObject : InteractableObject
{
    public override void OnInteract(GameObject interactor)
    {
        GameStateManager.Instance.AddDrink();
        gameObject.SetActive(false);
    }
}

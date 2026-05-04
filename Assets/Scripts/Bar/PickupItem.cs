using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic pickup for keys, bottles, and similar objects.
/// Disables itself on pickup; use OnPickedUp to hook into game logic.
/// </summary>
public class PickupItem : InteractableObject
{
    [Header("Pickup")]
    [SerializeField] private string itemName = "Item";

    public UnityEvent<PickupItem> OnPickedUp;

    public override string GetInteractionPrompt() => $"Pick up {itemName}";

    public override void OnInteract(GameObject interactor)
    {
        OnPickedUp?.Invoke(this);
        gameObject.SetActive(false);
    }
}

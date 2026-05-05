using UnityEngine;

// Replace PickupItem with this component on Key GameObjects.
public class KeyItem : PickupItem
{
    public override void OnInteract(GameObject interactor)
    {
        if (!BartenderNPC.introComplete) return;
        GameStateManager.Instance.PickupKey();
        base.OnInteract(interactor);
    }
}

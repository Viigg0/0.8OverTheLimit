using UnityEngine;

/// <summary>
/// Base class for all interactable objects. Handles highlight toggling via an outline material
/// added as an extra material slot on every child Renderer.
/// Subclass this and override OnInteract / GetInteractionPrompt.
/// </summary>
[DisallowMultipleComponent]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Left Click to interact";

    [Header("Outline")]
    [Tooltip("Assign the Material created from the Custom/URPOutline shader.")]
    [SerializeField] private Material outlineMaterial;

    private Renderer[] _renderers;
    private Material[][] _originalSharedMaterials;
    private bool _isHighlighted;

    protected virtual void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        _originalSharedMaterials = new Material[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
            _originalSharedMaterials[i] = _renderers[i].sharedMaterials;
    }

    public void SetHighlight(bool highlighted)
    {
        if (_isHighlighted == highlighted) return;
        _isHighlighted = highlighted;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (highlighted && outlineMaterial != null)
            {
                var original = _originalSharedMaterials[i];
                var mats = new Material[original.Length + 1];
                original.CopyTo(mats, 0);
                mats[mats.Length - 1] = outlineMaterial;
                _renderers[i].sharedMaterials = mats;
            }
            else
            {
                _renderers[i].sharedMaterials = _originalSharedMaterials[i];
            }
        }
    }

    public virtual string GetInteractionPrompt() => interactionPrompt;

    public virtual void OnInteract(GameObject interactor)
    {
        // Override in subclasses
    }

    private void OnDisable() => SetHighlight(false);
}

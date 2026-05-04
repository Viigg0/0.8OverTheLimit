using UnityEngine;

/// <summary>
/// Attach to the Main Camera. Raycasts from screen center each frame.
/// Highlights any InteractableObject in range and triggers interaction on left-click.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("UI")]
    [SerializeField] private InteractionPromptUI promptUI;

    private Camera _cam;
    private IInteractable _currentTarget;
    private InteractableObject _currentHighlighted;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        Debug.Log($"PlayerInteraction initialized. Camera: {_cam.gameObject.name}");
    }

    private void Update()
    {
        UpdateRaycast();

        if (_currentTarget != null && Input.GetMouseButtonDown(0))
            _currentTarget.OnInteract(gameObject);
    }

    private void UpdateRaycast()
    {
        // Cast from the exact centre of the viewport (crosshair position)
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayers))
        {
            Debug.Log($"HIT: {hit.collider.gameObject.name} at distance {hit.distance}");

            // Walk up the hierarchy so colliders on child meshes still resolve
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            var highlightable = interactable as InteractableObject;

            if (interactable != null)
            {
                Debug.Log($"Found IInteractable");

                // Already looking at the same object — nothing to change
                if (highlightable == _currentHighlighted)
                    return;

                ClearCurrentTarget();

                _currentTarget = interactable;
                _currentHighlighted = highlightable;
                _currentHighlighted?.SetHighlight(true);
                promptUI?.Show(_currentTarget.GetInteractionPrompt());
                return;
            }
            else
            {
                Debug.Log($"NO IInteractable on {hit.collider.gameObject.name}");
            }
        }

        ClearCurrentTarget();
    }

    private void ClearCurrentTarget()
    {
        if (_currentTarget == null) return;

        _currentHighlighted?.SetHighlight(false);
        _currentTarget = null;
        _currentHighlighted = null;
        promptUI?.Hide();
    }
}

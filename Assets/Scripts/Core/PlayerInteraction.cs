using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to the Main Camera. Raycasts from the mouse cursor position each frame.
/// Also owns the walk-to-door cinematic triggered on key pickup.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float interactionRadius = 0.1f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("UI")]
    [SerializeField] private InteractionPromptUI promptUI;

    // Walk-to-door destination
    private static readonly Vector3 DoorPosition = new Vector3(-6.64f, 5.17f, -5.73f);
    private const float DoorYaw = 187.134f;
    private const float TurnDuration = 1.0f;
    private const float StandDuration = 0.5f;
    private const float WalkDuration = 3.0f;
    private const float BounceAmplitude = 0.05f;
    private const float BounceFrequency = 2.0f;
    private const float IdleDuration = 2.0f;
    private const float IdleSwayAmplitude = 1.5f;   // degrees
    private const float IdleSwayFrequency = 1.2f;   // Hz
    private const float FadeDuration = 1.5f;

    private Camera _cam;
    private IInteractable _currentTarget;
    private InteractableObject _currentHighlighted;
    private bool _inputLocked;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnKeyPickedUp += TriggerWalkToDoor;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnKeyPickedUp -= TriggerWalkToDoor;
    }

    private void Update()
    {
        if (_inputLocked) return;

        if (_currentTarget != null && Mouse.current.leftButton.wasPressedThisFrame)
            _currentTarget.OnInteract(gameObject);

        UpdateRaycast();
    }

    private void UpdateRaycast()
    {
        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.SphereCast(ray, interactionRadius, out RaycastHit hit, interactionRange, interactableLayers))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            var highlightable = interactable as InteractableObject;

            if (interactable != null)
            {
                if (highlightable == _currentHighlighted)
                    return;

                ClearCurrentTarget();

                _currentTarget = interactable;
                _currentHighlighted = highlightable;
                _currentHighlighted?.SetHighlight(true);
                promptUI?.Show(_currentTarget.GetInteractionPrompt());
                return;
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

    // -------------------------------------------------------------------------
    //  Walk-to-door cinematic
    // -------------------------------------------------------------------------

    private void TriggerWalkToDoor() => StartCoroutine(WalkToDoorCoroutine());

    private IEnumerator WalkToDoorCoroutine()
    {
        _inputLocked = true;
        ClearCurrentTarget();

        var cameraSway = GetComponent<CameraSway>();
        if (cameraSway != null) cameraSway.enabled = false;

        Vector3 startPos = transform.position;
        float startYaw = transform.eulerAngles.y;
        float pitch = transform.eulerAngles.x;

        // Phase 1 — Ease-in/out turn (SmoothStep replaces linear Lerp)
        for (float t = 0f; t < TurnDuration; t += Time.deltaTime)
        {
            float smooth = Mathf.SmoothStep(0f, 1f, t / TurnDuration);
            float yaw = Mathf.LerpAngle(startYaw, DoorYaw, smooth);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(pitch, DoorYaw, 0f);

        // Phase 2 — Stand up from the stool
        float sitY = transform.position.y;
        for (float t = 0f; t < StandDuration; t += Time.deltaTime)
        {
            float y = Mathf.Lerp(sitY, DoorPosition.y, t / StandDuration);
            transform.position = new Vector3(startPos.x, y, startPos.z);
            yield return null;
        }

        // Phase 3 — Walk to door with footstep bounce
        Vector3 walkStart = new Vector3(startPos.x, DoorPosition.y, startPos.z);
        for (float t = 0f; t < WalkDuration; t += Time.deltaTime)
        {
            float bounceFade = t < WalkDuration - 0.5f ? 1f : (WalkDuration - t) / 0.5f;
            float bounce = Mathf.Sin(t * BounceFrequency * Mathf.PI * 2f) * BounceAmplitude * bounceFade;

            Vector3 pos = Vector3.Lerp(walkStart, DoorPosition, t / WalkDuration);
            pos.y += bounce;
            transform.position = pos;
            yield return null;
        }
        transform.position = DoorPosition;
        transform.rotation = Quaternion.Euler(pitch, DoorYaw, 0f);

        // Phase 4 — Idle sway at the door (breathing / standing feel)
        for (float t = 0f; t < IdleDuration; t += Time.deltaTime)
        {
            float sway = Mathf.Sin(t * IdleSwayFrequency * Mathf.PI * 2f) * IdleSwayAmplitude;
            transform.rotation = Quaternion.Euler(pitch, DoorYaw + sway, 0f);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(pitch, DoorYaw, 0f);

        // Phase 5 — Create a runtime black overlay and fade to black
        var fadeGO = new GameObject("__FadeOverlay");
        var fadeCanvas = fadeGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;
        var fadeImg = fadeGO.AddComponent<Image>();
        fadeImg.color = Color.black;
        var fadeCG = fadeGO.AddComponent<CanvasGroup>();
        fadeCG.alpha = 0f;
        fadeCG.blocksRaycasts = true;

        for (float t = 0f; t < FadeDuration; t += Time.deltaTime)
        {
            fadeCG.alpha = t / FadeDuration;
            yield return null;
        }
        fadeCG.alpha = 1f;

        SceneManager.LoadScene("Road");
    }
}

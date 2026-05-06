using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsOverlayUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CarMovement carMovement;
    [SerializeField] private float fadeDuration = 1f;

    private bool _dismissing;

    private void Start()
    {
        if (carMovement == null)
            carMovement = FindObjectOfType<CarMovement>();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        if (carMovement != null)
            carMovement.InputEnabled = false;
    }

    private void Update()
    {
        if (_dismissing) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        _dismissing = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        if (carMovement != null)
            carMovement.InputEnabled = true;
        gameObject.SetActive(false);
    }
}

using System.Collections;
using TMPro;
using UnityEngine;

public class SpeechBubbleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup      bubbleGroup;
    [SerializeField] private TextMeshProUGUI  bubbleText;
    [SerializeField] private CanvasGroup      clickPromptGroup;

    [Header("Timing")]
    [SerializeField] private float typeSpeed    = 0.035f;
    [SerializeField] private float fadeDuration = 0.18f;

    public bool IsOpen { get; private set; }

    private Coroutine _active;

    private void Awake()
    {
        Debug.Log($"[SpeechBubbleUI] Awake — bubbleGroup={(bubbleGroup == null ? "NULL" : "OK")}, bubbleText={(bubbleText == null ? "NULL" : "OK")}");

        SetupTailAnchor("TailOutline", 30f, 0);
        SetupTailAnchor("TailFill",    22f, 1);

        bubbleGroup.alpha          = 0f;
        bubbleGroup.blocksRaycasts = false;
    }

    private void SetupTailAnchor(string childName, float size, int siblingIndex)
    {
        if (bubbleGroup == null) return;
        var t = bubbleGroup.transform.Find(childName);
        if (t == null) return;

        t.SetSiblingIndex(siblingIndex);

        var r              = t.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(0f, 0.5f);
        r.anchorMax        = new Vector2(0f, 0.5f);
        r.pivot            = new Vector2(0.5f, 0.5f);
        r.sizeDelta        = new Vector2(size, size);
        r.anchoredPosition = Vector2.zero;
    }

    public void Show(string message)
    {
        Debug.Log($"[SpeechBubbleUI] Show — IsOpen was {IsOpen}, message='{message}'");
        IsOpen = true;
        if (_active != null) StopCoroutine(_active);
        _active = StartCoroutine(ShowRoutine(message));
    }

    public void Hide()
    {
        Debug.Log($"[SpeechBubbleUI] Hide called");
        IsOpen = false;
        if (_active != null) StopCoroutine(_active);
        _active = StartCoroutine(FadeGroup(bubbleGroup, bubbleGroup.alpha, 0f, () =>
        {
            bubbleGroup.blocksRaycasts = false;
            _active = null;
        }));
    }

    public void HideAndMarkIntroDone()
    {
        Hide();
    }

    private IEnumerator ShowRoutine(string message)
    {
        bubbleText.text            = "";
        bubbleGroup.blocksRaycasts = true;

        yield return FadeGroup(bubbleGroup, bubbleGroup.alpha, 1f, null);

        for (int i = 1; i <= message.Length; i++)
        {
            bubbleText.text = message.Substring(0, i);
            yield return new WaitForSeconds(typeSpeed);
        }
        _active = null;
    }

    // FadeGroup no longer touches _active — callers manage it.
    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, System.Action onDone)
    {
        float start = from;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(start, to, t / fadeDuration);
            yield return null;
        }
        group.alpha = to;
        onDone?.Invoke();
    }
}

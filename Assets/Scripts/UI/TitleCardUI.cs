using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Attach to a root GameObject in the TitleCard scene.
// Hierarchy expected:
//   TitleCardUI (this script + CanvasGroup "titleGroup")
//   └── TitleImage   (Image showing titlecard.png)
//   └── StartButton  (Button, wrapped in its own CanvasGroup "buttonGroup")
public class TitleCardUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup titleGroup;   // wraps the title card image
    [SerializeField] private CanvasGroup buttonGroup;  // wraps the Start button
    [SerializeField] private float       fadeDuration = 1.25f;

    private void Awake()
    {
        titleGroup.alpha          = 0f;
        buttonGroup.alpha         = 0f;
        buttonGroup.interactable  = false;
        buttonGroup.blocksRaycasts = false;
    }

    private void Start() => StartCoroutine(IntroSequence());

    private IEnumerator IntroSequence()
    {
        yield return Fade(titleGroup, 0f, 1f);
        yield return new WaitForSeconds(0.4f);
        yield return Fade(buttonGroup, 0f, 1f);
        buttonGroup.interactable   = true;
        buttonGroup.blocksRaycasts = true;
    }

    // Wired to the Start button's OnClick event in the Inspector
    public void OnStartClicked()
    {
        Debug.Log("TitleCardUI: OnStartClicked fired");
        Debug.Log($"TitleCardUI: titleGroup={titleGroup}, buttonGroup={buttonGroup}");
        buttonGroup.interactable   = false;
        buttonGroup.blocksRaycasts = false;
        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        Debug.Log("TitleCardUI: FadeOutAndLoad started");
        yield return Fade(buttonGroup, 1f, 0f);
        Debug.Log("TitleCardUI: button faded out");
        yield return Fade(titleGroup, 1f, 0f);
        Debug.Log("TitleCardUI: title faded out — loading CharacterSetup");
        SceneManager.LoadScene("CharacterSetup");
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed     += Time.deltaTime;
            group.alpha  = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}

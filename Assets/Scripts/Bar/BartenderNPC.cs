using UnityEngine;

public class BartenderNPC : InteractableObject
{
    [SerializeField] private SpeechBubbleUI speechBubble;

    private static readonly string[] IntroLines =
    {
        "Welcome to 0.8 Over The Limit. Click on me to continue!",
        "Your blood alcohol content is shown in the top left. It updates in real time.",
        "Your car keys are on the bar. Think carefully before you decide to drive.",
        "Click on me at any time to see how your current BAC level affects your ability to drive.",
        "You can now click the beers to increase your BAC, or grab your keys and drive home. The choice is yours."
    };

    public static bool introComplete;

    private int _introIndex;
    private float _lastClickTime = -4f;

    private void Start()
    {
        if (speechBubble == null)
        {
            speechBubble = FindObjectOfType<SpeechBubbleUI>();
            Debug.Log("speechBubble assigned: " + speechBubble);
        }

        if (speechBubble != null)
            speechBubble.Show(IntroLines[0], showMouseIcon: true);
    }

    public override string GetInteractionPrompt() => introComplete ? "Check your state" : "Left Click to Talk";

    public override void OnInteract(GameObject interactor)
    {
        if (Time.time - _lastClickTime < 2f) return;
        _lastClickTime = Time.time;

        Debug.Log($"[BartenderNPC] NPC clicked — introIndex={_introIndex}, introComplete={introComplete}");

        if (speechBubble == null) { Debug.LogError("[BartenderNPC] speechBubble is NULL — increment never reached"); return; }

        if (!introComplete)
        {
            _introIndex++;
            if (_introIndex >= IntroLines.Length - 1)
                introComplete = true;
        }
        else if (_introIndex < IntroLines.Length)
        {
            _introIndex = IntroLines.Length;
        }

        speechBubble.Show(_introIndex < IntroLines.Length ? IntroLines[_introIndex] : GetBACLine());
    }

    private string GetBACLine()
    {
        if (GameStateManager.Instance == null) return "...";
        float bac = GameStateManager.Instance.BAC;

        if (bac < 0.005f) return "You are completely sober, you are safe to drive.";
        if (bac < 0.02f) return "You are under the 0.02 BAC new driver limit but be careful, any more and new drivers face a ban.";
        if (bac < 0.05f) return "You are approaching the new driver limit of 0.02 BAC, new drivers should not drink any more.";
        if (bac < 0.08f) return "You are over the legal limit for new drivers at 0.02 BAC and over the general Danish limit of 0.05 BAC. You should not be driving.";
        if (bac < 0.15f) return "You are over the legal Danish limit of 0.05 BAC. Do not get in that car, fines and a license suspension await you.";
        return "You are dangerously over the limit. You are a serious risk to yourself and everyone on the road.";
    }
}

using UnityEngine;

public class BartenderNPC : InteractableObject
{
    [SerializeField] private SpeechBubbleUI speechBubble;

    private static readonly string[] IntroLines =
    {
        "Welcome to 0.8 Over The Limit.",
        "Your blood alcohol content is shown in the top left. It updates in real time.",
        "Your car keys are on the bar. Think carefully before you decide to drive.",
        "Click on me at any time to see how your current BAC level affects your ability to drive."
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
            speechBubble.Show(IntroLines[0]);
    }

    public override string GetInteractionPrompt() => introComplete ? "Check your state" : "Talk";

    public override void OnInteract(GameObject interactor)
    {
        if (Time.time - _lastClickTime < 4f) return;
        _lastClickTime = Time.time;

        Debug.Log($"[BartenderNPC] NPC clicked — introIndex={_introIndex}, introComplete={introComplete}");

        if (speechBubble == null) { Debug.LogError("[BartenderNPC] speechBubble is NULL — increment never reached"); return; }

        if (!introComplete)
        {
            Debug.Log("Before increment: " + _introIndex);
            _introIndex++;
            Debug.Log("After increment: " + _introIndex);
            if (_introIndex < IntroLines.Length)
                Debug.Log("Line to show: " + IntroLines[_introIndex]);
            if (_introIndex >= IntroLines.Length)
                introComplete = true;
        }

        speechBubble.Show(introComplete ? GetBACLine() : IntroLines[_introIndex]);
    }

    private string GetBACLine()
    {
        if (GameStateManager.Instance == null) return "...";
        float bac = GameStateManager.Instance.BAC;

        if (bac < 0.005f) return "You are completely sober, you are safe to drive.";
        if (bac < 0.02f) return "You are under the 0.2 per mille new driver limit but be careful, any more and new drivers face a ban.";
        if (bac < 0.05f) return "You are approaching the new driver limit of 0.2 per mille, new drivers should not drink any more.";
        if (bac < 0.08f) return "You are over the legal limit for new drivers at 0.2 per mille and over the general Danish limit of 0.5 per mille. You should not be driving.";
        if (bac < 0.15f) return "You are over the legal Danish limit of 0.5 per mille. Do not get in that car, fines and a license suspension await you.";
        return "You are dangerously over the limit. You are a serious risk to yourself and everyone on the road.";
    }
}

using TMPro;
using UnityEngine;

public class BACDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private static readonly Color Orange = new Color(1f, 0.5f, 0f);

    private void Start()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnBACChanged += UpdateText;

        label.text  = "BAC: 0.000%";
        label.color = Color.white;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnBACChanged -= UpdateText;
    }

    private void Update()
    {
        if (GameStateManager.Instance == null) return;
        Color target = TargetColor(GameStateManager.Instance.BAC);
        label.color = Color.Lerp(label.color, target, Time.deltaTime * 3f);
    }

    private void UpdateText(float bac) => label.text = $"BAC: {bac:F3}%";

    private Color TargetColor(float bac)
    {
        if (bac < 0.04f)
            return Color.white;
        if (bac < 0.08f)
            return Color.Lerp(Color.white, Color.yellow, Mathf.InverseLerp(0.04f, 0.08f, bac));
        if (bac < 0.15f)
            return Color.Lerp(Color.yellow, Orange, Mathf.InverseLerp(0.08f, 0.15f, bac));

        // 0.15+ — pulse between bright red and dark red
        float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
        return Color.Lerp(new Color(0.55f, 0f, 0f), Color.red, pulse);
    }
}

using TMPro;
using UnityEngine;

/// <summary>
/// Manages the interaction prompt shown at the bottom of the screen.
/// Assign this component to a UI Panel that contains a TextMeshProUGUI child.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI label;

    private void Awake() => Hide();

    public void Show(string prompt)
    {
        label.text = prompt;
        panel.SetActive(true);
    }

    public void Hide() => panel.SetActive(false);
}

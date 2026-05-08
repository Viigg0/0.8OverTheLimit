using TMPro;
using UnityEngine;

public class VictoryScreenUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI bacText;

    public void ShowVictory(float bac)
    {
        float actualBAC = GameStateManager.Instance != null ? GameStateManager.Instance.BAC : bac;

        if (panel != null) panel.SetActive(true);
        if (titleText != null) titleText.text = "You Made It Home!";
        if (bacText != null) bacText.text = $"BAC: {actualBAC:F3}";
        if (descriptionText != null)
        {
            if (actualBAC < 0.020f)
                descriptionText.text = "You made it home within the legal limit for new drivers. Well done.";
            else if (actualBAC < 0.050f)
                descriptionText.text = "You made it home within the legal limit for experienced drivers. However even small amounts of alcohol affect your reaction time.";
            else if (actualBAC < 0.100f)
                descriptionText.text = "You made it home drunk. This could have had serious consequences for you and everyone around you. This should never be done.";
            else
                descriptionText.text = "You made it home dangerously drunk. You put yourself and everyone on the road at serious risk. This is a criminal offence in Denmark.";
        }
        Time.timeScale = 0f;
    }

    public void HideVictory()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }
}

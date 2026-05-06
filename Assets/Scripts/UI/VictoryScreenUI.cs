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
        if (panel != null) panel.SetActive(true);
        if (titleText != null) titleText.text = "You Made It Home!";
        if (bacText != null) bacText.text = $"BAC: {bac:F3}";
        if (descriptionText != null)
        {
            if (bac < 0.04f)
                descriptionText.text = "You arrived home sober. Well done!";
            else if (bac < 0.08f)
                descriptionText.text = "You made it home slightly impaired. You were lucky this time.";
            else
                descriptionText.text = "You made it home drunk. This was dangerous and illegal!";
        }
        Time.timeScale = 0f;
    }

    public void HideVictory()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }
}

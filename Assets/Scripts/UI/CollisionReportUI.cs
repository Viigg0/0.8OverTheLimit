   
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollisionReportUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI bacText;
    public TMPro.TextMeshProUGUI speedText;
    public TMPro.TextMeshProUGUI severityText;

    // Call this from CollisionReport's onCollision event
    public void ShowReport(CollisionReport.CollisionResult result)
    {
        if (panel != null) panel.SetActive(true);
        if (titleText != null) titleText.text = result.title;
        if (descriptionText != null) descriptionText.text = result.description;
        if (bacText != null) bacText.text = $"BAC: {result.bac:F3}";
        if (speedText != null) speedText.text = $"Speed: {(result.impactSpeed * 3.6f):F1} km/h";
        if (severityText != null) severityText.text = $"Severity: {result.severity}";
        // Pause the game
        Time.timeScale = 0f;
    }

    // Optional: Hide the panel
    public void HideReport()
    {
        if (panel != null) panel.SetActive(false);
    }
     // Show the panel only (for On Game End event)
    public void ShowPanel()
    {
        if (panel != null) panel.SetActive(true);
    }
}

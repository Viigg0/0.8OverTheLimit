using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSetupUI : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float       fadeDuration = 1.25f;

    [Header("Gender")]
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;
    [SerializeField] private Button otherButton;
    [SerializeField] private Color  selectedColor   = Color.white;
    [SerializeField] private Color  unselectedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField ageInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private TMP_InputField weightInput;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI errorLabel;
    [SerializeField] private Button          confirmButton;

    // 0 = Male (r 0.68), 1 = Female (r 0.55), 2 = Other (r 0.55 — more conservative threshold)
    private int _genderIndex = 0;

    private void Awake() => canvasGroup.alpha = 0f;

    private void Start()
    {
        errorLabel.text = "";
        SetGenderIndex(0);
        StartCoroutine(Fade(0f, 1f));
    }

    public void SelectMale()   => SetGenderIndex(0);
    public void SelectFemale() => SetGenderIndex(1);
    public void SelectOther()  => SetGenderIndex(2);

    private void SetGenderIndex(int index)
    {
        _genderIndex = index;
        ApplyButtonState(maleButton,   index == 0);
        ApplyButtonState(femaleButton, index == 1);
        ApplyButtonState(otherButton,  index == 2);
    }

    private void ApplyButtonState(Button btn, bool selected)
    {
        var cb = btn.colors;
        cb.normalColor      = selected ? selectedColor : unselectedColor;
        cb.highlightedColor = selected ? selectedColor : new Color(0.5f, 0.5f, 0.5f, 1f);
        btn.colors = cb;
    }

    public void OnConfirm()
    {
        errorLabel.text = "";

        if (!int.TryParse(ageInput.text.Trim(), out int age) || age < 16 || age > 99)
        {
            errorLabel.text = "Enter a valid age (16–99).";
            return;
        }

        if (!float.TryParse(heightInput.text.Trim(), out float heightCm) || heightCm < 100f || heightCm > 250f)
        {
            errorLabel.text = "Enter a valid height in cm (100–250).";
            return;
        }

        float weightKg = 70f;
        string weightText = weightInput.text.Trim();
        if (!string.IsNullOrEmpty(weightText))
        {
            if (!float.TryParse(weightText, out weightKg) || weightKg < 30f || weightKg > 300f)
            {
                errorLabel.text = "Weight must be 30–300 kg, or leave blank (default 70 kg).";
                return;
            }
        }

        // Other uses female r-value (0.55) — more conservative biological threshold
        bool isMale = _genderIndex == 0;

        GameStateManager.Instance.SetPlayerStats(
            isMale:   isMale,
            age:      age,
            heightCm: heightCm,
            weightKg: weightKg);

        confirmButton.interactable = false;
        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        yield return Fade(1f, 0f);
        SceneManager.LoadScene("BarScene");
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed          += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}

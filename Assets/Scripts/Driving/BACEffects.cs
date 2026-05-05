using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BACEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarMovement carMovement;
    [SerializeField] private Volume postProcessVolume;

    [Header("Vignette")]
    [SerializeField] private float vignetteMinIntensity = 0f;
    [SerializeField] private float vignetteMaxIntensity = 0.65f;
    [SerializeField] private Color vignetteColor = new Color(0.1f, 0f, 0.15f);

    [Header("Blur (Depth of Field)")]
    [SerializeField] private float blurMinFocalLength = 300f;
    [SerializeField] private float blurMaxFocalLength = 2f;
    [SerializeField] private float blurMinAperture = 32f;
    [SerializeField] private float blurMaxAperture = 0.8f;

    [Header("Smoothing")]
    [SerializeField] private float effectSmoothSpeed = 2f;
    [SerializeField] private float effectActivationThreshold = 0.02f;

    private Vignette vignette;
    private DepthOfField depthOfField;
    private float smoothedBac01;

    void Start()
    {
        if (carMovement == null)
        {
            carMovement = GetComponentInParent<CarMovement>();
        }

        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
        }

        if (postProcessVolume == null)
        {
            Debug.LogError("BACEffects: No post-process Volume found. Assign one in the Inspector.");
            enabled = false;
            return;
        }

        if (!postProcessVolume.profile.TryGet(out vignette))
        {
            vignette = postProcessVolume.profile.Add<Vignette>(true);
        }

        if (!postProcessVolume.profile.TryGet(out depthOfField))
        {
            depthOfField = postProcessVolume.profile.Add<DepthOfField>(true);
        }

        // Ensure the effects start clean.
        depthOfField.mode.Override(DepthOfFieldMode.Bokeh);
        vignette.color.Override(vignetteColor);
        vignette.active = false;
        depthOfField.active = false;
    }

    void Update()
    {
        if (carMovement == null)
        {
            return;
        }

        float targetBac01 = GetNormalizedBAC();
        smoothedBac01 = Mathf.MoveTowards(smoothedBac01, targetBac01, effectSmoothSpeed * Time.deltaTime);

        bool effectsActive = smoothedBac01 > effectActivationThreshold;
        vignette.active = effectsActive;
        depthOfField.active = effectsActive;

        if (effectsActive)
        {
            ApplyVignette(smoothedBac01);
            ApplyBlur(smoothedBac01);
        }
    }

    private float GetNormalizedBAC()
    {
        // Mirror the same normalization used in CarMovement.
        return Mathf.Clamp01(carMovement.CurrentBAC / 0.2f);
    }

    private void ApplyVignette(float bac01)
    {
        float intensity = Mathf.Lerp(vignetteMinIntensity, vignetteMaxIntensity, bac01);
        vignette.intensity.Override(intensity);
    }

    private void ApplyBlur(float bac01)
    {
        // As BAC rises: focal length drops and aperture opens wide — creates bokeh blur.
        float focalLength = Mathf.Lerp(blurMinFocalLength, blurMaxFocalLength, bac01);
        float aperture = Mathf.Lerp(blurMinAperture, blurMaxAperture, bac01);

        depthOfField.focalLength.Override(focalLength);
        depthOfField.aperture.Override(aperture);

        // Focus distance stays at the player's view distance.
        depthOfField.focusDistance.Override(Mathf.Lerp(10f, 1.5f, bac01));
    }

    void OnDestroy()
    {
        if (vignette != null)
        {
            vignette.active = false;
        }

        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
    }
}

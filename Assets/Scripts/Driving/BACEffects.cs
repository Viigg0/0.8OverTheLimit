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

    [Header("Blur (Gaussian Depth of Field)")]
    [SerializeField] private float gaussianMaxBlurRadius = 1.5f;

    [Header("Smoothing")]
    [SerializeField] private float effectSmoothSpeed = 2f;
    [SerializeField] private float effectActivationThreshold = 0.02f;

    private Vignette vignette;
    private DepthOfField depthOfField;
    private float smoothedBac01;

    void Start()
    {
        float currentBAC = GameStateManager.Instance != null ? GameStateManager.Instance.BAC : 0f;
        Debug.Log("BACEffects Start — currentBAC: " + currentBAC + " | GetBlurIntensity: " + GetBlurIntensity(currentBAC));

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

        // Snap to the correct BAC level immediately — BAC is fixed for the entire drive.
        // Read from GameStateManager directly to avoid CarMovement.Start() ordering dependency.
        if (GameStateManager.Instance != null)
            smoothedBac01 = Mathf.Clamp01(GameStateManager.Instance.BAC / 0.15f);

        // Ensure the effects start clean.
        depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
        vignette.color.Override(vignetteColor);
        vignette.active = false;
        depthOfField.active = false;
    }

    void Update()
    {
        // Vignette — driven by smoothed bac01, requires carMovement.
        if (carMovement != null)
        {
            float targetBac01 = GetNormalizedBAC();
            smoothedBac01 = Mathf.MoveTowards(smoothedBac01, targetBac01, effectSmoothSpeed * Time.deltaTime);
            bool vignetteActive = smoothedBac01 > effectActivationThreshold;
            vignette.active = vignetteActive;
            if (vignetteActive) ApplyVignette(smoothedBac01);
        }

        // Blur — reads directly from GameStateManager, no carMovement dependency.
        if (GameStateManager.Instance != null)
        {
            bool blurActive = GameStateManager.Instance.BAC >= 0.040f;
            depthOfField.active = blurActive;
            if (blurActive) ApplyBlur();
        }
    }

    private float GetNormalizedBAC()
    {
        return Mathf.Clamp01(carMovement.CurrentBAC / 0.15f);
    }

    private void ApplyVignette(float bac01)
    {
        float intensity = Mathf.Lerp(vignetteMinIntensity, vignetteMaxIntensity, bac01);
        vignette.intensity.Override(intensity);
    }

    private void ApplyBlur()
    {
        float bac = GameStateManager.Instance.BAC;
        if (depthOfField != null)
        {
            float intensity = GetBlurIntensity(bac);
            // Gaussian DOF: blur everything by keeping start/end at 0 and scaling max radius.
            depthOfField.gaussianStart.Override(0f);
            depthOfField.gaussianEnd.Override(0.01f);
            depthOfField.gaussianMaxRadius.Override(Mathf.Lerp(0f, gaussianMaxBlurRadius, intensity));
            depthOfField.active = bac >= 0.040f;
        }
    }

    private float GetBlurIntensity(float bac)
    {
        if (bac < 0.040f) return 0.0f;
        if (bac < 0.060f) return 0.2f;
        if (bac < 0.090f) return 0.4f;
        if (bac < 0.120f) return 0.7f;
        return 1.0f;
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

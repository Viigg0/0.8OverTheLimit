using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Attach to any GameObject in the scene. Assign the scene's Global Volume in the Inspector.
// The Volume profile must have ColorAdjustments and ChromaticAberration effects added.
public class IntoxicationPostFX : MonoBehaviour
{
    [SerializeField] private Volume volume;

    private ColorAdjustments  _colorAdjustments;
    private ChromaticAberration _chromaticAberration;

    private void Start()
    {
        if (volume == null) return;

        // Instantiate a runtime copy so the original profile asset is never dirtied
        volume.profile = Instantiate(volume.profile);

        volume.profile.TryGet(out _colorAdjustments);
        volume.profile.TryGet(out _chromaticAberration);
    }

    private void Update()
    {
        float bac = GameStateManager.Instance != null ? GameStateManager.Instance.BAC : 0f;
        float t   = Mathf.Clamp01(bac / 0.15f);
        float dt  = Time.deltaTime * 2f;

        if (_colorAdjustments != null)
        {
            // Saturation: 0 (sober) → 60 (heavily drunk — colours feel overwhelming)
            float target = Mathf.Lerp(0f, 60f, t);
            _colorAdjustments.saturation.value = Mathf.Lerp(_colorAdjustments.saturation.value, target, dt);
        }

        if (_chromaticAberration != null)
        {
            // Chromatic aberration: 0 → 0.6 (colour fringing on screen edges)
            float target = Mathf.Lerp(0f, 0.6f, t);
            _chromaticAberration.intensity.value = Mathf.Lerp(_chromaticAberration.intensity.value, target, dt);
        }
    }
}

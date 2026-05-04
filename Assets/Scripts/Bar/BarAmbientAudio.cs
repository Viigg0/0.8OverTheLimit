using UnityEngine;

// Attach to an empty GameObject in BarScene.
// Assign Assets/Audio/SFX/Ambient.mp3 to the Ambient Clip field.
// RequireComponent auto-adds AudioSource and AudioLowPassFilter when you add this script.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class BarAmbientAudio : MonoBehaviour
{
    [SerializeField] private AudioClip ambientClip;

    [Header("Volume")]
    [SerializeField] private float soberVolume = 0.75f;  // natural bar level
    [SerializeField] private float drunkVolume = 1.0f;   // bar feels louder when tipsy

    private const float CleanCutoff   = 22000f;  // effectively unfiltered
    private const float MuffledCutoff = 800f;    // heavily muffled at BAC 0.15+

    private AudioSource        _source;
    private AudioLowPassFilter _lowPass;

    private void Awake()
    {
        _source              = GetComponent<AudioSource>();
        _source.clip         = ambientClip;
        _source.loop         = true;
        _source.playOnAwake  = false;
        _source.spatialBlend = 0f;      // 2D — ambient, not positional
        _source.volume       = soberVolume;
        _source.Play();

        _lowPass                  = GetComponent<AudioLowPassFilter>();
        _lowPass.cutoffFrequency  = CleanCutoff;
        _lowPass.lowpassResonanceQ = 1f;
    }

    private void Update()
    {
        float bac = GameStateManager.Instance != null ? GameStateManager.Instance.BAC : 0f;
        float t   = Mathf.Clamp01(bac / 0.15f);
        float dt  = Time.deltaTime * 2f;

        // Volume: subtle ramp from sober to drunk level
        _source.volume = Mathf.Lerp(
            _source.volume,
            Mathf.Lerp(soberVolume, drunkVolume, t),
            dt);

        // Cutoff: 22000 Hz (clean) → 800 Hz (muffled like hearing through water)
        _lowPass.cutoffFrequency = Mathf.Lerp(
            _lowPass.cutoffFrequency,
            Mathf.Lerp(CleanCutoff, MuffledCutoff, t),
            dt);

        // Slight resonance increase adds an "underwater" quality at high BAC
        _lowPass.lowpassResonanceQ = Mathf.Lerp(
            _lowPass.lowpassResonanceQ,
            Mathf.Lerp(1f, 3.5f, t),
            dt);
    }
}

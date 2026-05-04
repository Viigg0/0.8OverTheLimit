using UnityEngine;

// Attach to the Main Camera. Offsets local position with a sine wave scaled by BAC.
[RequireComponent(typeof(Camera))]
public class CameraSway : MonoBehaviour
{
    [SerializeField] private float maxAmplitude = 0.12f;  // world-unit peak offset at max BAC
    [SerializeField] private float maxFrequency = 1.8f;   // oscillations per second at max BAC

    private Vector3 _baseLocalPosition;
    private float _amplitude;
    private float _frequency = 0.3f;

    private void Awake() => _baseLocalPosition = transform.localPosition;

    private void Update()
    {
        float bac = GameStateManager.Instance != null ? GameStateManager.Instance.BAC : 0f;
        float t = Mathf.Clamp01(bac / 0.15f);

        // Lerp amplitude and frequency so intensity ramps up smoothly when BAC changes
        _amplitude  = Mathf.Lerp(_amplitude,  Mathf.Lerp(0f, maxAmplitude, t), Time.deltaTime * 1.5f);
        _frequency  = Mathf.Lerp(_frequency,  Mathf.Lerp(0.3f, maxFrequency, t), Time.deltaTime * 1.5f);

        // Two sine waves with different phases so X and Y feel independent
        float swayX = Mathf.Sin(Time.time * _frequency)               * _amplitude;
        float swayY = Mathf.Sin(Time.time * _frequency * 0.6f + 1.1f) * _amplitude * 0.4f;

        transform.localPosition = _baseLocalPosition + new Vector3(swayX, swayY, 0f);
    }
}

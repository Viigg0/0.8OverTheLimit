using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour
{
    [Header("Base Driving")]
    [SerializeField] private float acceleration = 22f;
    [SerializeField] private float brakeAcceleration = 32f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float steeringDegreesPerSecond = 90f;
    [SerializeField] private float idleDrag = 0.2f;

    [Header("BAC")]
    [SerializeField] [Range(0f, 0.3f)] private float currentBAC;
    [SerializeField] private float bacForMaxImpairment = 0.2f;

    [Header("Reaction Delay")]
    [SerializeField] private float baseReactionDelay = 0f;
    [SerializeField] private float maxReactionDelay = 0.7f;

    [Header("BAC Driving Impact")]
    [SerializeField] [Range(0f, 1f)] private float steeringLossAtMaxBAC = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float topSpeedLossAtMaxBAC = 0.35f;
    [SerializeField] private float steerDelayAtMaxBAC = 0.35f;
    [SerializeField] private float swayFrequency = 1.8f;
    [SerializeField] private float swaySteerAmountAtMaxBAC = 0.35f;

    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private float smoothedSteerInput;
    private float swaySeed;

    private struct InputSample
    {
        public float timestamp;
        public Vector2 input;
    }

    private readonly Queue<InputSample> inputBuffer = new Queue<InputSample>();
    private Vector2 delayedInput;

    public float CurrentBAC => currentBAC;
    public bool InputEnabled = false;

    void Start()
    {
        if (GameStateManager.Instance != null)
        {
            SetBAC(GameStateManager.Instance.BAC);
        }

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        swaySeed = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float bac01 = GetNormalizedBAC();
        float reactionDelay = Mathf.Lerp(baseReactionDelay, maxReactionDelay, bac01);

        // Record raw input stamped with current time.
        inputBuffer.Enqueue(new InputSample { timestamp = Time.time, input = ReadMoveInput() });

        // Release samples whose delay has elapsed into delayedInput.
        while (inputBuffer.Count > 0 && Time.time - inputBuffer.Peek().timestamp >= reactionDelay)
        {
            delayedInput = inputBuffer.Dequeue().input;
        }

        throttleInput = delayedInput.y;
        steerInput = delayedInput.x;
    }

    private Vector2 ReadMoveInput()
    {
        if (!InputEnabled) return Vector2.zero;
        Vector2 result = Vector2.zero;

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            result = gamepad.leftStick.ReadValue();
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) result.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) result.y -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) result.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) result.x -= 1f;
        }

        return Vector2.ClampMagnitude(result, 1f);
    }

    void FixedUpdate()
    {
        float bac01 = GetNormalizedBAC();

        ApplyDrive(bac01);
        ApplySteering(bac01);
        ClampSpeed(bac01);
    }

    public void AddBAC(float amount)
    {
        currentBAC = Mathf.Max(0f, currentBAC + amount);
    }

    public void SetBAC(float value)
    {
        currentBAC = Mathf.Max(0f, value);
    }

    private float GetNormalizedBAC()
    {
        if (bacForMaxImpairment <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(currentBAC / bacForMaxImpairment);
    }

    private void ApplyDrive(float bac01)
    {
        float forwardInput = Mathf.Clamp(throttleInput, -1f, 1f);
        if (Mathf.Approximately(forwardInput, 0f))
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(-horizontalVelocity * idleDrag, ForceMode.Acceleration);
            return;
        }

        float accel = forwardInput >= 0f ? acceleration : brakeAcceleration;
        float enginePenalty = Mathf.Lerp(1f, 0.7f, bac01);
        float driveForce = forwardInput * accel * enginePenalty;
        rb.AddForce(transform.forward * driveForce, ForceMode.Acceleration);
    }

    private void ApplySteering(float bac01)
    {
        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (planarVelocity.sqrMagnitude < 0.01f)
        {
            return;
        }

        float sway = Mathf.Sin((Time.time * swayFrequency) + swaySeed) * swaySteerAmountAtMaxBAC * bac01;
        float targetSteerInput = Mathf.Clamp(steerInput + sway, -1f, 1f);

        float steerResponseTime = Mathf.Lerp(0.04f, steerDelayAtMaxBAC, bac01);
        float steerLerp = 1f - Mathf.Exp(-Time.fixedDeltaTime / Mathf.Max(0.0001f, steerResponseTime));
        smoothedSteerInput = Mathf.Lerp(smoothedSteerInput, targetSteerInput, steerLerp);

        float steerAuthority = Mathf.Lerp(1f, 1f - steeringLossAtMaxBAC, bac01);
        float steerAmount = smoothedSteerInput * steeringDegreesPerSecond * steerAuthority * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, steerAmount, 0f));
    }

    private void ClampSpeed(float bac01)
    {
        float effectiveMaxSpeed = maxSpeed * Mathf.Lerp(1f, 1f - topSpeedLossAtMaxBAC, bac01);
        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (planarVelocity.magnitude <= effectiveMaxSpeed)
        {
            return;
        }

        Vector3 clampedPlanar = planarVelocity.normalized * effectiveMaxSpeed;
        rb.linearVelocity = new Vector3(clampedPlanar.x, rb.linearVelocity.y, clampedPlanar.z);
    }
}

using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{

    public static GameStateManager Instance { get; private set; }

    // Metric Widmark formula: BAC% = (drinks × 1.4) / (weightKg × r) − (0.015 × hours)
    // 1.4 = 14 g alcohol per standard drink × 100 / 1000 (g→kg normalisation)
    private const float GramsAlcoholConstant = 1.4f;
    private const float MetabolismPerHour    = 0.015f;

    // Player stats — defaults used if CharacterSetup is skipped (e.g. Play-in-Editor on BarScene)
    private float _weightKg  = 70f;
    private float _widmarkR  = 0.68f;  // male default; female = 0.55
    private int   _age       = 30;
    private float _heightCm  = 175f;
    private bool  _isMale    = true;

    public int   DrinksConsumed { get; private set; }
    public float BAC            { get; private set; }
    public bool  HasKey         { get; private set; }

    public event Action<float> OnBACChanged;
    public event Action        OnKeyPickedUp;

    private float _firstDrinkTime = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Called by CharacterSetupUI after the player fills in the form
    public void SetPlayerStats(bool isMale, int age, float heightCm, float weightKg)
    {
        _isMale   = isMale;
        _age      = age;
        _heightCm = heightCm;
        _weightKg = weightKg;
        _widmarkR = isMale ? 0.68f : 0.55f;
        RecalculateBAC();
    }

    public void AddDrink()
    {
        if (_firstDrinkTime < 0f)
            _firstDrinkTime = Time.time;

        DrinksConsumed++;
        RecalculateBAC();
    }

    public void PickupKey()
    {
        HasKey = true;
        OnKeyPickedUp?.Invoke();
    }

    public void ResetBAC()
    {
        DrinksConsumed = 0;
        BAC = 0f;
        HasKey = false;
        _firstDrinkTime = -1f;
        OnBACChanged?.Invoke(BAC);
    }

    private void RecalculateBAC()
    {
        float hoursElapsed = _firstDrinkTime >= 0f
            ? (Time.time - _firstDrinkTime) / 3600f
            : 0f;

        BAC = Mathf.Max(0f,
            (DrinksConsumed * GramsAlcoholConstant) / (_weightKg * _widmarkR)
            - MetabolismPerHour * hoursElapsed);

        OnBACChanged?.Invoke(BAC);
    }
}

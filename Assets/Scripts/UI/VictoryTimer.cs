using UnityEngine;
using UnityEngine.Events;

public class VictoryTimer : MonoBehaviour
{
    public float victoryTime = 120f; // 2 minutes
    public UnityEvent onVictory;
    private float timer = 0f;
    private bool isActive = true;

    void Update()
    {
        if (!isActive) return;
        timer += Time.deltaTime;
        if (timer >= victoryTime)
        {
            isActive = false;
            onVictory?.Invoke();
        }
    }

    public void StopTimer()
    {
        isActive = false;
    }

    public float GetElapsedTime()
    {
        return timer;
    }
}

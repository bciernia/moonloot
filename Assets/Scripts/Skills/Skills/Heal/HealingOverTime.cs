using System.Collections;
using UnityEngine;

public class HealingOverTime : MonoBehaviour
{
    private PlayerHealth _playerHealth;
    private float _duration;
    private float _tickInterval;
    private float _healPerTick;

    public void Initialize(PlayerHealth playerHealth, float totalDuration, float totalHeal)
    {
        _playerHealth = playerHealth;
        _duration = totalDuration;
        _tickInterval = 1f;
        _healPerTick = totalHeal / (_duration / _tickInterval);

        StartCoroutine(HealOverTimeCoroutine());
    }

    private IEnumerator HealOverTimeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            _playerHealth.RestoreHealth(_healPerTick);
            elapsed += _tickInterval;
            yield return new WaitForSeconds(_tickInterval);
        }

        Destroy(this);
    }
}
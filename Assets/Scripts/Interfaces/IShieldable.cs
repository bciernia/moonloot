using UnityEngine;

public interface IShieldable
{
    void ReduceDamage(float amount, float duration, GameObject effect);
}
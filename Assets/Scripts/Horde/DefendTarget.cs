using UnityEngine;

public class DefendTarget : MonoBehaviour, IDamageable
{
    public float hp = 100f;

    private void Awake()
    {
        if (hp == 0) hp = 100f;
    }

    public void TakeDamage(float amount, Transform damageSourceTransform = null, DamageType type = DamageType.Physical)
    {
        if (damageSourceTransform != null && !damageSourceTransform.CompareTag("Enemy")) return;
        
        hp -= amount;
        
        if(hp <= 0) HordeManager.Instance.FailDefendObjective();
    }
}
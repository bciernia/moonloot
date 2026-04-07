using UnityEngine;

public class DefendTarget : MonoBehaviour, IDamageable
{
    public float currentHp = 100f;
    public float maxHp = 100f;
    
    
    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float amount, Transform damageSourceTransform = null, DamageType type = DamageType.Physical)
    {
        if (damageSourceTransform != null && !damageSourceTransform.CompareTag("Enemy")) return;
        
        currentHp -= amount;
        
        if(currentHp <= 0) HordeManager.Instance.FailDefendObjective();
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "Shield_", menuName = "Skill/Otton/Shield")]
public class Shield : Skill
{
    [SerializeField] public GameObject ShieldEffect;
    [SerializeField] public float Duration;
    [SerializeField] public float ReduceAmount;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;
        var mana = user.GetComponent<PlayerMana>();
        if (mana != null && !mana.TryUseMana(ManaCost)) return false;
        
        var playerHealth = user.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("There is no player health for Shield Spell");
            return false;
        }
        
        playerHealth.ReduceDamage(ReduceAmount, Duration, ShieldEffect);
        return true;
    }
}
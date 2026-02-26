using UnityEngine;

[CreateAssetMenu(fileName = "Rage_", menuName = "Skill/Rage")]
public class Rage : Skill
{
    public float DmgMultiplier = 2f;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;

        var hp = user.GetComponent<PlayerHealth>();
        if (hp != null && hp.CurrentHealth <= HealthCost)
        {
            Debug.Log("Hp is too low");
            return false;
        }

        hp?.TakeDamage(HealthCost);

        var playerAttack = user.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.ApplyDmgMultiplier(DmgMultiplier, ActiveTime);
        }

        return true;
    }
}

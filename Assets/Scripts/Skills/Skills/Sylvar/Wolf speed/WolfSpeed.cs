using UnityEngine;

[CreateAssetMenu(fileName = "WolfSpeed_", menuName = "Skill/WolfSpeed")]
public class WolfSpeed : Skill
{
    public float SpeedMultiplier = 2f;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;

        var mana = user.GetComponent<PlayerMana>();
        if (mana != null && mana.CurrentMana < ManaCost)
        {
            Debug.Log("No mana");
            return false;
        }

        mana?.UseMana(ManaCost);

        var movement = user.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ApplySpeedMultiplier(SpeedMultiplier, ActiveTime);
        }

        return true;
    }
}

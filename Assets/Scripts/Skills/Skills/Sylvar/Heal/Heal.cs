using UnityEngine;

[CreateAssetMenu(fileName = "Heal_", menuName = "Skill/Heal")]
public class Heal : Skill
{
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

        base.Activate(user);
        
        return true;
    }
}
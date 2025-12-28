using UnityEngine;

[CreateAssetMenu(fileName = "Heal_", menuName = "Skill/Heal")]
public class Heal : Skill
{
    public override void Activate(GameObject user)
    {
        if (user == null) return;
        var mana = user.GetComponent<PlayerMana>();
        if (mana != null && mana.CurrentMana < ManaCost)
        {
            Debug.Log("No mana");
            return;
        }

        mana?.UseMana(ManaCost);

        base.Activate(user);
    }
}
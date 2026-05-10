using UnityEngine;

[CreateAssetMenu(fileName = "Heal_", menuName = "Skill/Heal")]
public class Heal : Skill
{
    [SerializeField] private float healAmount;

    public override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        var mana = user.GetComponent<PlayerMana>();

        if (mana != null && !mana.TryUseMana(ManaCost))
            return false;

        base.Activate(user);

        return true;
    }
}
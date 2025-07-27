using UnityEngine;

[CreateAssetMenu(fileName = "Heal_", menuName = "Skill/Heal")]
public class Heal : Skill
{
    [Header("Heal configuration")]
    [SerializeField] private float healthAmount;
    [SerializeField] private GameObject healingVisualEffect;
    
    public override void Activate(GameObject user)
    {
        var userMana = user.GetComponent<PlayerMana>();
        
        if (userMana.CurrentMana < ManaCost)
        {
            Debug.Log("No mana");
            return;
        }

        var userHealth = user.GetComponent<PlayerHealth>();
        userMana.UseMana(ManaCost);
        
        var healing = user.AddComponent<HealingOverTime>();
        healing.Initialize(userHealth, ActiveTime, healthAmount);
        
        var instance = Instantiate(healingVisualEffect, user.transform.position, Quaternion.identity);
        instance.transform.SetParent(user.transform);

        var controller = instance.GetComponent<SkillVisualEffectController>();
        controller.Duration = ActiveTime;
    }
}

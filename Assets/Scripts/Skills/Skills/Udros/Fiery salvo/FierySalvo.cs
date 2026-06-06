using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "FierySalvo_", menuName = "Skill/FierySalvo")]
public class FierySalvo : Skill
{
    [SerializeField] private Projectile _projectileSo;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float interval = 1f;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;

        var attack = user.GetComponent<PlayerAttack>();
        if (attack == null) return false;

        attack.StartCoroutine(FireRoutine(user, attack.firePoint));
        return true;
    }
    
    private IEnumerator FireRoutine(GameObject user, Transform firePoint)
    {
        var elapsed = 0f;

        var modifiedDuration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                duration
            );
        
        while (elapsed < modifiedDuration)
        {
            FireProjectile(user, firePoint);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void FireProjectile(GameObject user, Transform firePoint)
    {
        var projectile = Instantiate(_projectileSo, firePoint.position, firePoint.rotation);
        projectile.Shooter = user;
        projectile.IsEnemy = false;
        projectile.Direction = Vector3.right;
        projectile.Damage = _projectileSo.Damage;
    }

    public override string GetDescription()
    {
        var skillDuration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, duration);
        
        return
            $"Udros ignites your wrath.\nYou unleash a barrage of fireballs, one each second, culminating in a devastating fivefold inferno. Let the battlefield burn for {skillDuration} seconds.";
    }
}

using UnityEngine;
using UnityEngine.Serialization;

public class ActionRangedAttack : FSMAction
{
    public Transform firePoint;
    public GameObject slashEffect;
    [SerializeField] private Weapon _weapon;
    
    private EnemyBrain _enemyBrain;
    
    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    public override void Act()
    {
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (!_enemyBrain.Player) return;

        if (_enemyBrain.CanAttack())
        {
            _enemyBrain.ResetAttackCooldown();

            var slashObject = Instantiate(slashEffect, firePoint.position, firePoint.rotation);
            var enemySlash = slashObject.GetComponent<EnemySlash>();
            enemySlash.SetParent(firePoint);
            enemySlash.SetShooter(gameObject);
        } 
    }
}
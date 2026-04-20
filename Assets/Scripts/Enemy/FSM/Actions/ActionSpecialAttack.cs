using UnityEngine;

public class ActionSpecialAttack : FSMAction
{
    private EnemyStatistics _enemyStatistics;
    
    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override void Act()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var specialAttack = Instantiate(_enemyStatistics.SpecialAttacks[0], player.transform.position, Quaternion.identity);
        var spell = specialAttack.GetComponent<Spell>();

        if (spell != null)
        {
            spell.Shooter = _enemyStatistics.gameObject;
        }
    }
}

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
        Debug.Log("SPELL");
        var player = GameObject.FindGameObjectWithTag("Player");
        Instantiate(_enemyStatistics.SpecialAttacks[0], player.transform.position, Quaternion.identity);        
    }
}

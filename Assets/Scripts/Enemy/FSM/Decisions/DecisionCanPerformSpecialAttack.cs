using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionCanPerformSpecialAttack : FSMDecision
{
    private EnemyStatistics _enemyStatistics;
    private bool _canUseSpecialAttack;
    
    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
        _canUseSpecialAttack = true;
    }

    public override bool Decide()
    {
        if (_canUseSpecialAttack && _enemyStatistics.SpecialAttackTimeInterval <= 0f)
        {
            Debug.Log("CanPerformSpecialAttack");
            
            StartCoroutine(SpecialAttackCooldown());
            return true;
        }

        _enemyStatistics.SpecialAttackTimeInterval -= Time.deltaTime;
        Debug.Log("Nie może");
        Debug.Log(_enemyStatistics.SpecialAttackTimeInterval);
        return false;    
    }

    private IEnumerator SpecialAttackCooldown()
    {
        _canUseSpecialAttack = false;

        _enemyStatistics.SpecialAttackTimeInterval = _enemyStatistics.MaxAttackTimeInterval;

        while (_enemyStatistics.SpecialAttackTimeInterval > 0f)
        {
            _enemyStatistics.SpecialAttackTimeInterval -= Time.deltaTime;
            yield return null;
        }

        _canUseSpecialAttack = true;
    }
}
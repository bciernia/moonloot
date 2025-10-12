using System;
using UnityEngine;

public class EnemyRelationship : MonoBehaviour
{
    [SerializeField] private bool IsFriendly = false;

    private EnemyBrain _enemyBrain;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    public bool IsCharacterFriendly() => IsFriendly;

    public void SetIsCharacterFriendly(bool isFriendly)
    {
        IsFriendly = isFriendly;
    }

    public void SetCharacterAsEnemy()
    {
        _enemyBrain.SetEnemyLayer();
        _enemyBrain.SetEnemyTag();
        IsFriendly = false;
    }
}

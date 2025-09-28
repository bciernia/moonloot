using System;
using UnityEngine;

public class EnemyRelationship : MonoBehaviour
{
    [SerializeField] private bool IsFriendly = false;

    public bool IsCharacterFriendly() => IsFriendly;

    public void SetCharacterFriendly(bool isFriendly)
    {
        IsFriendly = isFriendly;
    }
}

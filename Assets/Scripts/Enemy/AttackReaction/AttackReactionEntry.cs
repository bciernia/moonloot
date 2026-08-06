using System;
using UnityEngine;

[Serializable]
public class AttackReactionEntry
{
    public AttackReactionType Effect;

    [Range(0,100)]
    public float Chance;

    public float Value;
}
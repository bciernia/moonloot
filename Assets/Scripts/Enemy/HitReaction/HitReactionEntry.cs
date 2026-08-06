using System;
using UnityEngine;

[Serializable]
public class HitReactionEntry
{
    public HitReactionType Effect;

    [Range(0, 100)]
    public float Chance;

    public float Duration;

    public float Distance;
}
using UnityEngine;

[System.Serializable]
public class DeathEffectEntry
{
    public DeathEffectType Effect;

    [Range(0,100)]
    public float Chance;

    public float Radius = 3f;
}
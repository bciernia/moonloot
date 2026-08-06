using UnityEngine;

[System.Serializable]
public class DeathEffectData
{
    public DeathEffectType Type;

    public GameObject EffectPrefab;
    public GameObject RangePrefab;

    public FreezingEffect FreezingEffect;
    public PoisonEffect PoisonEffect;
    
    public AudioClip Sound;
}
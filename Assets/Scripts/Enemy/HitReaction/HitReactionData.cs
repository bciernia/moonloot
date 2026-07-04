using UnityEngine;

[System.Serializable]
public class HitReactionData
{
    public HitReactionType Type;

    public GameObject StartEffectPrefab;
    public GameObject EndEffectPrefab;

    public AudioClip Sound;
}
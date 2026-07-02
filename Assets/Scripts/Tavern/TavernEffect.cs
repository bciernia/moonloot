using System;

[Serializable]
public class TavernEffect
{
    public TavernEffectType Effect;

    public TavernEffectTiming Timing;

    public int Value = 1;
}
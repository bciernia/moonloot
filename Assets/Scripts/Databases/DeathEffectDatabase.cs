using System.Collections.Generic;
using UnityEngine;

public class DeathEffectDatabase : Singleton<DeathEffectDatabase>
{
    [SerializeField]
    private List<DeathEffectData> _effects;

    private Dictionary<DeathEffectType, DeathEffectData> _lookup;

    protected override void Awake()
    {
        base.Awake();

        _lookup = new Dictionary<DeathEffectType, DeathEffectData>();

        foreach (var effect in _effects)
        {
            _lookup.Add(effect.Type, effect);
        }
    }

    public DeathEffectData Get(DeathEffectType type)
    {
        if (!_lookup.TryGetValue(type, out var data))
            return null;

        return data;
    }
}
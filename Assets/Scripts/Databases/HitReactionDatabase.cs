using System.Collections.Generic;
using UnityEngine;

public class HitReactionDatabase : Singleton<HitReactionDatabase>
{
    [SerializeField]
    private List<HitReactionData> _reactions;

    private Dictionary<HitReactionType, HitReactionData> _lookup;

    protected override void Awake()
    {
        base.Awake();

        _lookup = new Dictionary<HitReactionType, HitReactionData>();

        foreach (var reaction in _reactions)
        {
            _lookup[reaction.Type] = reaction;
        }
    }

    public HitReactionData Get(HitReactionType type)
    {
        _lookup.TryGetValue(type, out var data);
        return data;
    }
}
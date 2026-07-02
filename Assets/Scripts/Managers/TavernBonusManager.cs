using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TavernBonusManager : Singleton<TavernBonusManager>
{
    private readonly Dictionary<TavernEffectType, int> _effects
        = new();

    public int GetValue(TavernEffectType effect) => _effects.GetValueOrDefault(effect, 0);
    
    private void AddEffect(TavernEffect effect)
    {
        if (_effects.ContainsKey(effect.Effect))
        {
            _effects[effect.Effect] += effect.Value;
        }
        else
        {
            _effects.Add(
                effect.Effect,
                effect.Value);
        }
    }
    
    private void Clear()
    {
        _effects.Clear();
    }
    
    public void Rebuild()
    {
        Clear();

        foreach (var roomData in TavernManager.Instance.RoomDatas)
        {
            var roomSO =
                TavernManager.Instance.GetRoom(roomData.RoomId);

            if (roomSO == null)
                continue;

            foreach (var effect in roomSO.Effects)
            {
                AddEffect(effect);
            }

            foreach (var upgradeId in roomData.PurchasedUpgrades)
            {
                var upgrade =
                    roomSO.Upgrades.FirstOrDefault(
                        x => x.Id == upgradeId);

                if (upgrade == null)
                    continue;

                foreach (var effect in upgrade.Effects)
                {
                    AddEffect(effect);
                }
            }
        }
        
        foreach (var effect in _effects)
        {
            Debug.Log($"{effect.Key} = {effect.Value}");
        }
    }
}
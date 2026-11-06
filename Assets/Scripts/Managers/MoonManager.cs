using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoonManager : Singleton<MoonManager>
{
    [SerializeField] private List<MoonData> moons;

    public MoonData CurrentMoon { get; private set; }

    public Action<MoonData> OnMoonChanged;
    
    private void Start()
    {
        if (CurrentMoon == null)
        {
            RollMoon();
        }
    }

    public void RollMoon()
    {
        CurrentMoon = moons[Random.Range(0, moons.Count)];
        OnMoonChanged?.Invoke(CurrentMoon);
    }

    public MoonData SetCurrentMoon(MoonType type)
    {
        if (moons == null)
            return null;

        CurrentMoon = moons.FirstOrDefault(moon => moon != null && moon.Type == type);
        if (CurrentMoon != null)
            OnMoonChanged?.Invoke(CurrentMoon);

        return CurrentMoon;
    }
    
    public List<MoonData> GetRandomMoons(int count)
    {
        if (moons == null || moons.Count == 0)
            return new List<MoonData>();

        var availableMoons = moons
            .OrderBy(_ => Random.value)
            .Take(count)
            .ToList();

        return availableMoons;
    }
}

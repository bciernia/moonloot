using System;
using System.Collections.Generic;
using UnityEngine;

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
        // CurrentMoon = moons[Random.Range(0, moons.Count)];
        CurrentMoon = moons[4];

        Debug.Log($"Current moon: {CurrentMoon.DisplayName}");

        OnMoonChanged?.Invoke(CurrentMoon);
    }
}
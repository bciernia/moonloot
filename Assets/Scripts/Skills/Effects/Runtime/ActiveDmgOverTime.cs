using UnityEngine;
using System;

public class ActiveDmgOverTime : ActiveEffect
{
    private float tickInterval;
    private float tickTimer;
    private Action<GameObject> onTick;
    private GameObject target;

    public void InitializeEffect(Effect effect, GameObject target, GameObject uiObj, Action<GameObject> onTickAction)
    {
        this.target = target;
        this.onTick = onTickAction;
        tickInterval = effect.TickInterval;
        
        Initialize(effect, uiObj, null);
        tickTimer = 0f;
    }

    protected override void Tick(float deltaTime)
    {
        tickTimer += deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            onTick?.Invoke(target);
        }
    }

    public void ResetEffect()
    {
        timer = 0f;
        tickTimer = 0f;
    }
}
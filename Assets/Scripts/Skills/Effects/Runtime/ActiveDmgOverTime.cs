using UnityEngine;
using System;

public class ActiveDmgOverTime : ActiveEffect
{
    private float tickInterval;
    private float tickTimer;
    private Action<GameObject> onTick;
    private GameObject target;
    private IHealth health;
    private bool _isFinished;
    
    public void InitializeEffect(Effect effect, GameObject target, GameObject uiObj, Action<GameObject> onTickAction)
    {
        this.target = target;
        health = target.GetComponent<IHealth>();
        onTick = onTickAction;
        tickInterval = effect.TickInterval;
        
        Initialize(effect, uiObj, null);
        tickTimer = 0f;
    }

    protected override void Tick(float deltaTime)
    {
        if (_isFinished || target == null || health == null || !health.IsAlive)
        {
            FinishEffect();
            return;
        }

        tickTimer += deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;

            if (target == null || !health.IsAlive)
            {
                FinishEffect();
                return;
            }

            onTick?.Invoke(target);
        }
    }

    public void ResetEffect()
    {
        timer = 0f;
        tickTimer = 0f;
    }
    
    private void FinishEffect()
    {
        _isFinished = true;
    }
}
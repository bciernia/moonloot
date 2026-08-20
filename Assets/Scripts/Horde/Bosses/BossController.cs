using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private readonly List<IBossSkill> _skills = new();
    private readonly HashSet<IBossSkill> _executedSkills = new();
    private EnemyStatistics _stats;
    
    private void Awake()
    {
        _skills.AddRange(GetComponents<IBossSkill>());
        _stats = GetComponent<EnemyStatistics>();
    }

    private void Update()
    {
        if (_stats == null || !_stats.IsAlive)
            return;

        CheckSkills();
    }

    private void CheckSkills()
    {
        var healthPercent = _stats.CurrentHP / _stats.MaxHP;

        foreach (var skill in _skills)
        {
            if (skill.ExecuteOnce && _executedSkills.Contains(skill))
                continue;

            if (healthPercent > skill.HealthThreshold)
                continue;

            if (!skill.CanExecute())
                continue;

            skill.Execute();

            if (skill.ExecuteOnce)
            {
                _executedSkills.Add(skill);
            }
        }
    }
}
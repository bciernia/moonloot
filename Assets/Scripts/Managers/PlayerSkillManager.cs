using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSkillManager : Singleton<PlayerSkillManager>, ISaveable
{
    [Header("Debug")]
    [SerializeField] private bool logUnlocks = true;

    [Header("Starting Skills")]
    [SerializeField] private List<Skill> startingSkills = new();

    private readonly HashSet<string> _unlockedSkillIds = new();
    private readonly List<Skill> _unlockedSkills = new();

    public IReadOnlyList<Skill> UnlockedSkills => _unlockedSkills;

    private Dictionary<string, List<SkillStatModifier>> _skillModifiers = new();

    public event Action OnSkillsChanged;

    private bool _isLoaded = false;

    private void Start()
    {
        if (!_isLoaded)
        {
            InitializeStartingSkills();
        }
    }

    public float GetSkillStat(
        Skill skill,
        SkillStatType statType,
        float baseValue)
    {
        if (skill == null)
            return baseValue;

        if (!_skillModifiers.TryGetValue(skill.Id, out var modifiers))
            return baseValue;

        var final = baseValue;

        foreach (var modifier in modifiers)
        {
            if (modifier.statType == statType)
            {
                final += modifier.value;
            }
        }

        return final;
    }
    
    public void AddSkillModifier(
        Skill skill,
        SkillStatType type,
        float value)
    {
        if (skill == null)
            return;

        if (!_skillModifiers.ContainsKey(skill.Id))
        {
            _skillModifiers[skill.Id] =
                new List<SkillStatModifier>();
        }

        _skillModifiers[skill.Id].Add(
            new SkillStatModifier
            {
                statType = type,
                value = value
            });

        Debug.Log($"Added {type} modifier to {skill.Name}: {value}");
    }
    
    public bool IsUnlocked(Skill skill)
    {
        return skill != null && _unlockedSkillIds.Contains(skill.Id);
    }

    public bool UnlockSkill(Skill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("Trying to unlock NULL skill");
            return false;
        }

        if (_unlockedSkillIds.Contains(skill.Id))
        {
            if (logUnlocks)
                Debug.Log($"Skill already unlocked: {skill.name}");
            return false;
        }

        _unlockedSkillIds.Add(skill.Id);
        _unlockedSkills.Add(skill);

        if (logUnlocks)
            Debug.Log($"Unlocked skill: {skill.name}");

        OnSkillsChanged?.Invoke();
        return true;
    }

    public void ResetSkills()
    {
        _unlockedSkillIds.Clear();
        _unlockedSkills.Clear();
        OnSkillsChanged?.Invoke();
    }

    private void InitializeStartingSkills()
    {
        foreach (var skill in startingSkills)
        {
            UnlockSkill(skill);
        }

        if (logUnlocks)
            Debug.Log("Initialized starting skills");
    }
    

    #region SAVE / LOAD

    public void Save()
    {
        ES3.Save("player_unlocked_skills", _unlockedSkillIds.ToList());
    }

    public void Load()
    {
        if (!ES3.KeyExists("player_unlocked_skills"))
        {
            _isLoaded = false;
            return;
        }

        var ids = ES3.Load<List<string>>("player_unlocked_skills");

        _unlockedSkillIds.Clear();
        _unlockedSkills.Clear();

        foreach (var id in ids)
        {
            var skill = SkillDatabase.Get(id);

            if (skill == null)
            {
                Debug.LogWarning($"Skill not found in database: {id}");
                continue;
            }

            _unlockedSkillIds.Add(id);
            _unlockedSkills.Add(skill);
        }

        _isLoaded = true;

        OnSkillsChanged?.Invoke();
    }

    #endregion
}
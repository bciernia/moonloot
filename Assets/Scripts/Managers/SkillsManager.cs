using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class SkillEntry
{
    public Skill skill;
    public Image cooldownImage;
}

internal class SkillRuntimeData
{
    public float cooldownTimer;
    public float activeTimer;
    public SkillState state = SkillState.ready;
}

public class SkillsManager : Singleton<SkillsManager>, ISaveable
{
    [Header("Slots")]
    public List<SkillEntry> skills = new();
    public GameObject user;

    [Header("UI")]
    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;

    [Header("Targeting")]
    [SerializeField] private GameObject RangeIndicatorPrefab;

    private RangeIndicator _currentIndicator;
    private int _aimingSkillIndex = -1;
    [SerializeField] private int slotCount = 2;

    private PlayerInput _playerInput;

    private Action<InputAction.CallbackContext> _skill1Started;
    private Action<InputAction.CallbackContext> _skill1Canceled;
    private Action<InputAction.CallbackContext> _skill2Started;
    private Action<InputAction.CallbackContext> _skill2Canceled;

    // 🔥 Runtime per skill (nie per slot)
    private Dictionary<Skill, SkillRuntimeData> _skillRuntime =
        new Dictionary<Skill, SkillRuntimeData>();

    #region INITIALIZATION

    protected override void Awake()
    {
        base.Awake();

        _playerInput = FindAnyObjectByType<PlayerInput>();

        _skill1Started  = ctx => OnSkillStarted(0);
        _skill1Canceled = ctx => OnSkillReleased(0);

        _skill2Started  = ctx => OnSkillStarted(1);
        _skill2Canceled = ctx => OnSkillReleased(1);

        // Tworzymy runtime dla startowych skilli
        foreach (var entry in skills)
        {
            if (entry.skill != null)
                GetRuntime(entry.skill);
        }
        
        while (skills.Count < slotCount)
        {
            skills.Add(new SkillEntry());
        }

        RefreshSlotUI();
    }

    private void OnEnable()
    {
        if (_playerInput == null) return;

        _playerInput.actions["Skill1"].started  += _skill1Started;
        _playerInput.actions["Skill1"].canceled += _skill1Canceled;

        _playerInput.actions["Skill2"].started  += _skill2Started;
        _playerInput.actions["Skill2"].canceled += _skill2Canceled;
    }

    private void OnDisable()
    {
        if (_playerInput == null) return;

        _playerInput.actions["Skill1"].started  -= _skill1Started;
        _playerInput.actions["Skill1"].canceled -= _skill1Canceled;

        _playerInput.actions["Skill2"].started  -= _skill2Started;
        _playerInput.actions["Skill2"].canceled -= _skill2Canceled;
    }

    #endregion

    #region RUNTIME

    private SkillRuntimeData GetRuntime(Skill skill)
    {
        if (!_skillRuntime.TryGetValue(skill, out var runtime))
        {
            runtime = new SkillRuntimeData();
            _skillRuntime.Add(skill, runtime);
        }

        return runtime;
    }

    private void Update()
    {
        // Aktualizacja runtime per skill
        foreach (var runtime in _skillRuntime.Values)
        {
            switch (runtime.state)
            {
                case SkillState.active:
                    if (runtime.activeTimer > 0)
                        runtime.activeTimer -= Time.deltaTime;
                    else
                        runtime.state = SkillState.cooldown;
                    break;

                case SkillState.cooldown:
                    if (runtime.cooldownTimer > 0)
                        runtime.cooldownTimer -= Time.deltaTime;
                    else
                        runtime.state = SkillState.ready;
                    break;
            }
        }

        UpdateSlotUI();
    }

    #endregion

    #region SKILL INPUT

    private void OnSkillStarted(int index)
    {
        if (index < 0 || index >= skills.Count || GameManager.Instance.CurrentMode == GameMode.WorldMap)
            return;

        var entry = skills[index];
        if (entry.skill == null)
            return;

        if (!PlayerSkillManager.Instance.IsUnlocked(entry.skill))
            return;

        var runtime = GetRuntime(entry.skill);
        if (runtime.state != SkillState.ready)
            return;

        if (entry.skill.TargetingType == TargetType.Self)
        {
            ActivateSkill(entry.skill);
            return;
        }

        _aimingSkillIndex = index;

        if (RangeIndicatorPrefab != null)
        {
            var obj = Instantiate(
                RangeIndicatorPrefab,
                user.transform.position,
                Quaternion.identity,
                user.transform
            );

            _currentIndicator = obj.GetComponent<RangeIndicator>();

            _currentIndicator.SetRangeIndicatorRadius(entry.skill.Radius);

            _currentIndicator.SetRangeIndicatorColor(
                entry.skill.RangeIndicatorMinColor,
                entry.skill.RangeIndicatorMaxColor
            );

            _currentIndicator.transform.localPosition = Vector3.zero;
        }
    }

    private void OnSkillReleased(int index)
    {
        if (_aimingSkillIndex != index)
            return;

        var entry = skills[index];
        if (entry.skill == null)
            return;

        ActivateSkill(entry.skill);

        if (_currentIndicator != null)
            Destroy(_currentIndicator.gameObject);

        _currentIndicator = null;
        _aimingSkillIndex = -1;
    }

    #endregion

    #region ACTIVATE

    private void ActivateSkill(Skill skill)
    {
        if (!skill.Activate(user))
            return;

        var runtime = GetRuntime(skill);

        runtime.state = SkillState.active;
        runtime.activeTimer = skill.ActiveTime;
        runtime.cooldownTimer = skill.Cooldown;

        SoundManager.Instance.PlaySound(skill.SFX, 1f);
    }

    #endregion

    #region UI

    public void RefreshSlotUI()
    {
        RefreshSingleSlot(0, QSkillImage);
        RefreshSingleSlot(1, ESkillImage);
    }

    private void RefreshSingleSlot(int index, Image image)
    {
        if (index >= skills.Count)
        {
            image.gameObject.SetActive(false);
            return;
        }

        var entry = skills[index];

        if (entry.skill == null)
        {
            image.gameObject.SetActive(false);
            return;
        }

        image.sprite = entry.skill.Icon;
        image.gameObject.SetActive(true);
    }

    private void UpdateSlotUI()
    {
        for (int i = 0; i < skills.Count; i++)
        {
            var entry = skills[i];

            if (entry.cooldownImage == null)
                continue;

            if (entry.skill == null)
            {
                entry.cooldownImage.fillAmount = 1f;
                continue;
            }

            var runtime = GetRuntime(entry.skill);

            if (runtime.state == SkillState.cooldown && entry.skill.Cooldown > 0)
            {
                entry.cooldownImage.fillAmount =
                    1f - (runtime.cooldownTimer / entry.skill.Cooldown);
            }
            else if (runtime.state == SkillState.active && entry.skill.ActiveTime > 0)
            {
                entry.cooldownImage.fillAmount =
                    runtime.activeTimer / entry.skill.ActiveTime;
            }
            else
            {
                entry.cooldownImage.fillAmount = 1f;
            }
        }
    }

    #endregion

    #region SAVE AND LOAD
    
    public void Save()
    {
        var saveList = new List<SkillSaveData>();

        foreach (var entry in skills)
        {
            if (entry.skill == null)
            {
                saveList.Add(new SkillSaveData());
                continue;
            }

            var runtime = GetRuntime(entry.skill);

            saveList.Add(new SkillSaveData
            {
                skillID = entry.skill.Id,
                cooldownTimer = runtime.cooldownTimer,
                activeTimer = runtime.activeTimer,
                state = runtime.state
            });
        }

        ES3.Save("player_chosen_skills", saveList);
    }

    public void Load()
    {
        if (!ES3.KeyExists("player_chosen_skills"))
            return;

        var loaded = ES3.Load<List<SkillSaveData>>("player_chosen_skills");

        _skillRuntime.Clear();

        for (var i = 0; i < skills.Count && i < loaded.Count; i++)
        {
            var data = loaded[i];

            skills[i].skill = SkillDatabase.Get(data.skillID);

            if (data.skillID == null)
                continue;

            var runtime = GetRuntime(skills[i].skill);

            runtime.cooldownTimer = data.cooldownTimer;
            runtime.activeTimer = data.activeTimer;
            runtime.state = data.state;
        }

        RefreshSlotUI();
    }
    
    [Serializable]
    public class SkillSaveData
    {
        public string skillID;
        public float cooldownTimer;
        public float activeTimer;
        public SkillState state;
    }
    
    #endregion
}
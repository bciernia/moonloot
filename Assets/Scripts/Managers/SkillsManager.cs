using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class SkillEntry
{
    public Skill skill;
    public Image cooldownImage;

    [NonSerialized] public float cooldownTimer;
    [NonSerialized] public float activeTimer;
    [NonSerialized] public SkillState state = SkillState.ready;
}

internal class ActiveEffects
{
    public SkillEntry Skill;
    public GameObject EffectObject;
}

public class SkillsManager : Singleton<SkillsManager>
{
    [Header("Skill Config")]
    public List<SkillEntry> skills = new List<SkillEntry>();
    public GameObject user;

    [Header("UI References")]
    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;

    [Header("Effect Prefabs")]
    [SerializeField] private GameObject EffectsContainer;
    [SerializeField] private GameObject EffectPrefab;

    [Header("Range Indicator")]
    [SerializeField] private GameObject RangeIndicatorPrefab;

    private RangeIndicator _currentIndicator;
    private int _aimingSkillIndex = -1;
    
    private List<ActiveEffects> activeEffects = new List<ActiveEffects>();

    private PlayerInput _playerInput;

    private bool _skillsEnabled = true;
    
    private Action<InputAction.CallbackContext> _skill1Started;
    private Action<InputAction.CallbackContext> _skill1Canceled;

    private Action<InputAction.CallbackContext> _skill2Started;
    private Action<InputAction.CallbackContext> _skill2Canceled;
    
    protected override void Awake()
    {
        base.Awake();

        _playerInput = FindAnyObjectByType<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("SkillsManager: PlayerInput nie znaleziony!");
            return;
        }

        // przypisujemy callbacki (raz!)
        _skill1Started  = ctx => StartSkill(0);
        _skill1Canceled = ctx => ReleaseSkill(0);

        _skill2Started  = ctx => StartSkill(1);
        _skill2Canceled = ctx => ReleaseSkill(1);

        if (skills.Count > 0) QSkillImage.sprite = skills[0].skill.Icon;
        if (skills.Count > 1) ESkillImage.sprite = skills[1].skill.Icon;
    }
    
    private void OnEnable()
    {
#pragma warning disable UDR0005
        GameManager.OnGameModeChanged += OnGameModeChanged;
#pragma warning restore UDR0005

        if (_playerInput == null) return;

        _playerInput.actions["Skill1"].started  += _skill1Started;
        _playerInput.actions["Skill1"].canceled += _skill1Canceled;

        _playerInput.actions["Skill2"].started  += _skill2Started;
        _playerInput.actions["Skill2"].canceled += _skill2Canceled;
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        GameManager.OnGameModeChanged -= OnGameModeChanged;

        if (_playerInput == null) return;

        _playerInput.actions["Skill1"].started  -= _skill1Started;
        _playerInput.actions["Skill1"].canceled -= _skill1Canceled;

        _playerInput.actions["Skill2"].started  -= _skill2Started;
        _playerInput.actions["Skill2"].canceled -= _skill2Canceled;
    }
    
    private void OnGameModeChanged(GameMode mode)
    {
        _skillsEnabled = (mode == GameMode.Location);

        if (!_skillsEnabled)
        {
            CancelAllSkills();
        }
    }
    
    private void CancelAllSkills()
    {
        foreach (var entry in skills)
        {
            entry.activeTimer = 0f;
            entry.cooldownTimer = 0f;
            entry.state = SkillState.ready;

            if (entry.cooldownImage != null)
                entry.cooldownImage.fillAmount = 1f;
        }

        foreach (var effect in activeEffects)
        {
            if (effect.EffectObject != null)
                Destroy(effect.EffectObject);
        }

        activeEffects.Clear();
    }

    private void TryActivateSkill(int index)
    {
        if (!_skillsEnabled) return;
        if (index < 0 || index >= skills.Count) return;

        var entry = skills[index];
        if (!entry.skill.IsInUse) return;
        if (entry.state != SkillState.ready) return;

        var isSkillActivated = entry.skill.Activate(user);

        if (isSkillActivated) return;
        
        entry.state = SkillState.active;
        entry.activeTimer = entry.skill.ActiveTime;

        SoundManager.Instance.PlaySound(entry.skill.SFX, 1f);
        
        if (entry.skill.Effect != null)
        {
            entry.skill.Effect.Apply(user);

            var effectUI = StatusEffectUIManager.Instance.CreateEffectUI(entry.skill.Effect);

            activeEffects.Add(new ActiveEffects
            {
                Skill = entry,
                EffectObject = effectUI
            });
        }
    }

    private void Update()
    {
        foreach (var entry in skills)
        {
            switch (entry.state)
            {
                case SkillState.active:
                    if (entry.activeTimer > 0)
                    {
                        entry.activeTimer -= Time.deltaTime;

                        if (entry.skill.ActiveTime > 0)
                        {
                            var progress = entry.activeTimer / entry.skill.ActiveTime;
                            entry.cooldownImage.fillAmount = progress;
                        }
                    }
                    else
                    {
                        entry.state = SkillState.cooldown;
                        entry.cooldownTimer = entry.skill.Cooldown;

                        var toRemove = activeEffects.Find(e => e.Skill == entry);
                        if (toRemove != null)
                        {
                            activeEffects.Remove(toRemove);
                            Destroy(toRemove.EffectObject);
                        }
                    }
                    break;

                case SkillState.cooldown:
                    if (entry.cooldownTimer > 0)
                    {
                        entry.cooldownTimer -= Time.deltaTime;

                        if (entry.skill.Cooldown > 0)
                        {
                            var progress = 1f - (entry.cooldownTimer / entry.skill.Cooldown);
                            entry.cooldownImage.fillAmount = progress;
                        }
                    }
                    else
                    {
                        entry.state = SkillState.ready;
                        entry.cooldownImage.fillAmount = 1f;
                    }
                    break;
            }
        }
    }
    
    private void StartSkill(int index)
    {
        if (!_skillsEnabled) return;
        if (index < 0 || index >= skills.Count) return;

        var entry = skills[index];
        if (!entry.skill.IsInUse) return;
        if (entry.state != SkillState.ready) return;

        if (entry.skill.TargetingType == TargetType.Self)
        {
            ActivateEntry(entry);
            return;
        }

        entry.state = SkillState.aiming;
        _aimingSkillIndex = index;

        var obj = Instantiate(
            RangeIndicatorPrefab,
            user.transform.position,
            Quaternion.identity,
            user.transform   
        );

        _currentIndicator = obj.GetComponent<RangeIndicator>();
        _currentIndicator.SetRangeIndicatorRadius(entry.skill.Radius);
        _currentIndicator.SetRangeIndicatorColor(entry.skill.RangeIndicatorMinColor, entry.skill.RangeIndicatorMaxColor);
        _currentIndicator.transform.localPosition = Vector3.zero;
    }
    
    private void ReleaseSkill(int index)
    {
        if (_aimingSkillIndex != index) return;

        var entry = skills[index];
        if (entry.state != SkillState.aiming) return;

        ActivateEntry(entry);

        Destroy(_currentIndicator.gameObject);
        _currentIndicator = null;
        _aimingSkillIndex = -1;
    }
    
    private void ActivateEntry(SkillEntry entry)
    {
        var isSkillActivated = entry.skill.Activate(user);

        if (!isSkillActivated) return;
        
        entry.state = SkillState.active;
        entry.activeTimer = entry.skill.ActiveTime;

        SoundManager.Instance.PlaySound(entry.skill.SFX, 1f);

        if (entry.skill.Effect != null)
        {
            entry.skill.Effect.Apply(user);

            var effectUI =
                StatusEffectUIManager.Instance.CreateEffectUI(entry.skill.Effect);

            activeEffects.Add(new ActiveEffects
            {
                Skill = entry,
                EffectObject = effectUI
            });
        }
    }
}

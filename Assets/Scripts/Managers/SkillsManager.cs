using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class ActionSlot
{
    public ActionType type;
    public Skill skill;
    public Image cooldownImage;
    public int quickSlotIndex;
}

public enum ActionType
{
    Skill,
    QuickItem
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
    public List<ActionSlot> slots = new();
    public GameObject user;

    [Header("UI")]
    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;
    [SerializeField] private Image QuickItem1Image;
    [SerializeField] private Image QuickItem2Image;

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
    private Action<InputAction.CallbackContext> _quickItem1Started;
    private Action<InputAction.CallbackContext> _quickItem1Canceled;
    private Action<InputAction.CallbackContext> _quickItem2Started;
    private Action<InputAction.CallbackContext> _quickItem2Canceled;

    // 🔥 Runtime per skill (nie per slot)
    private Dictionary<Skill, SkillRuntimeData> _skillRuntime =
        new Dictionary<Skill, SkillRuntimeData>();

    #region INITIALIZATION

    protected override void Awake()
    {
        base.Awake();

        _playerInput = FindAnyObjectByType<PlayerInput>();

        _skill1Started  = ctx => OnSlotStarted(0);
        _skill1Canceled = ctx => OnSlotReleased(0);

        _skill2Started  = ctx => OnSlotStarted(1);
        _skill2Canceled = ctx => OnSlotReleased(1);
        
        _quickItem1Started  = ctx => OnSlotStarted(2);
        _quickItem1Canceled = ctx => OnSlotReleased(2);
        
        _quickItem2Started  = ctx => OnSlotStarted(3);
        _quickItem2Canceled = ctx => OnSlotReleased(3);

        // Tworzymy runtime dla startowych skilli
        foreach (var entry in slots)
        {
            if (entry.skill == null)
            {
                Debug.LogWarning("Empty skill slot detected");
                continue;
            }

            Debug.Log("Skill loaded: " + entry.skill.Name);

            GetRuntime(entry.skill);
        }
        
        while (slots.Count < slotCount)
        {
            slots.Add(new ActionSlot());
        }

    }

    private IEnumerator Start()
    {
        yield return null;

        _playerInput = FindAnyObjectByType<PlayerInput>();

        if (_playerInput == null)
        {
            Debug.LogError("Player input not found");
            yield break;
        }

        RegisterInputs();
        
        RefreshSlotUI();
        
        Debug.Log("SKILLS MANAGER INITIALIZED");
    }
    
    private void RegisterInputs()
    {
        _playerInput.actions["Skill1"].started += _skill1Started;
        _playerInput.actions["Skill1"].canceled += _skill1Canceled;

        _playerInput.actions["Skill2"].started += _skill2Started;
        _playerInput.actions["Skill2"].canceled += _skill2Canceled;

        _playerInput.actions["QuickItem1"].started += _quickItem1Started;
        _playerInput.actions["QuickItem1"].canceled += _quickItem1Canceled;

        _playerInput.actions["QuickItem2"].started += _quickItem2Started;
        _playerInput.actions["QuickItem2"].canceled += _quickItem2Canceled;

        Debug.Log("INPUTS REGISTERED");
        
    }
    
    private void OnDestroy()
    {
        if (_playerInput == null)
            return;

        _playerInput.actions["Skill1"].started -= _skill1Started;
        _playerInput.actions["Skill1"].canceled -= _skill1Canceled;

        _playerInput.actions["Skill2"].started -= _skill2Started;
        _playerInput.actions["Skill2"].canceled -= _skill2Canceled;

        _playerInput.actions["QuickItem1"].started -= _quickItem1Started;
        _playerInput.actions["QuickItem1"].canceled -= _quickItem1Canceled;

        _playerInput.actions["QuickItem2"].started -= _quickItem2Started;
        _playerInput.actions["QuickItem2"].canceled -= _quickItem2Canceled;
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

    private void OnSlotStarted(int index)
    {
        Debug.Log("SLOT STARTED: " + index);
        
        if (index < 0 || index >= slots.Count || GameManager.Instance.CurrentMode == GameMode.WorldMap)
            return;

        var slot = slots[index];

        switch (slot.type)
        {
            case ActionType.Skill:
                HandleSkillStarted(index);
                break;
            case ActionType.QuickItem:
                UseQuickItem(slot.quickSlotIndex);
                break;
            default:
                Debug.LogWarning("Unknown action type in slot: " + slot.type);
                break;
        }
    }

    private void UseQuickItem(int index)
    {
        var equipped = EquippedItemsManager.Instance;
        var item = equipped.EquippedItems[index];

        if (item.IsEmpty) return;

        var action = item.item as IItemAction;
        if (action == null) return;

        var player = GameManager.Instance.Player.gameObject;
        action.PerformAction(player, item, true);
        item.quantity--;
        
        equipped.EquippedItems[index] = item;

        if (item.quantity <= 0)
        {
            equipped.EquippedItems[index] = InventoryItem.GetEmptyItem();
        }
        else
        {
            equipped.EquippedItems[index] = item;
        }

        RefreshSlotUI();
        QuickItemManager.Instance.RefreshUI();
        EquippedItemsManager.Instance.InitializeEquippedSlots();
    }

    private void HandleSkillStarted(int index)
    {
        var entry = slots[index];
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

    private void OnSlotReleased(int index)
    {
        if (_aimingSkillIndex != index)
            return;

        var entry = slots[index];
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
        Debug.Log("Skill activate: " + skill.Name);

        
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
        RefreshSingleSlot(2, QuickItem1Image);
        RefreshSingleSlot(3, QuickItem2Image);
    }

    private void RefreshSingleSlot(int index, Image image)
    {
        if (index >= slots.Count)
        {
            image.gameObject.SetActive(false);
            return;
        }

        var slot = slots[index];

        switch (slot.type)
        {
            case ActionType.Skill:
                if (slot.skill == null)
                {
                    image.gameObject.SetActive(false);
                    return;
                }

                image.sprite = slot.skill.Icon;
                image.gameObject.SetActive(true);
                break;

            case ActionType.QuickItem:
                var item = EquippedItemsManager.Instance.EquippedItems[slot.quickSlotIndex];

                if (item.IsEmpty)
                {
                    image.gameObject.SetActive(false);
                    return;
                }

                image.sprite = item.item.Image;
                image.gameObject.SetActive(true);
                break;
        }
    }

    private void UpdateSlotUI()
    {
        for (var i = 0; i < slots.Count; i++)
        {
            var entry = slots[i];

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

        foreach (var entry in slots)
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

        for (var i = 0; i < slots.Count && i < loaded.Count; i++)
        {
            var data = loaded[i];

            slots[i].skill = SkillDatabase.Get(data.skillID);

            if (data.skillID == null)
                continue;

            var runtime = GetRuntime(slots[i].skill);

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
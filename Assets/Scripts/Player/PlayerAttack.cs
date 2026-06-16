using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [FormerlySerializedAs("weapon")] [SerializeField]
    private WeaponItemSO _weapon;

    [SerializeField] private PlayerStatsSO _playerStats;
    [SerializeField] private Image cooldownImage;
    
    [SerializeField] private ItemParameterSO damageBonusParameter;

    [SerializeField] private float damageVariancePercent = 0.1f;
    
    public Transform firePoint;
    public GameObject slashEffect;
    private float attackCooldown;
    private bool canAttack = true;
    private PlayerMana _playerMana;
    private PlayerStamina _playerStamina;
    private PlayerInput _playerInput;
    private SlashEffect _slash;
    
    private float _currentDmgMultiplier = 1f;
    private float _attackCooldownMultiplier = 1f;
    private void Awake()
    {
        _playerMana = GetComponent<PlayerMana>();
        _playerStamina = GetComponent<PlayerStamina>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        WeaponManager.Instance.SetWeapon(EquippedItemsManager.Instance.EquippedItems[0].item as WeaponItemSO, null);
        ArmorManager.Instance.SetArmor(EquippedItemsManager.Instance.EquippedItems[1].item as ArmorItemSO, null);
        ArmorManager.Instance.SetHelmet(EquippedItemsManager.Instance.EquippedItems[3].item as HelmetItemSO, null);
        ArmorManager.Instance.SetShoes(EquippedItemsManager.Instance.EquippedItems[4].item as ShoesItemSO, null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(_playerStats.TotalDamage);
        }
    }

    private void Attack()
    {
        if (!canAttack || DialogueManager.Instance.IsInDialogue() ||
            GameManager.Instance.CurrentMode == GameMode.WorldMap || PauseManager.Instance.IsGamePaused) return;

        var manaCost = _weapon.ProjectilePrefab ? _weapon.ProjectilePrefab.ProjectileSo.ManaCost : 0f;

        var canPerformAttack = CanPerformAttack(manaCost, _weapon.RequiredStamina, _playerMana.CurrentMana, _playerStamina.CurrentStamina);
        if (!canPerformAttack) return;

        FireSlashEffect();
    }

    private void FireSlashEffect()
    {
        var slashObject = Instantiate(slashEffect, firePoint.position, firePoint.rotation);
        var slash = slashObject.GetComponent<SlashEffect>();
        slash.SetShooter(gameObject);
        CreateSlashEffect(slash); 
    }

    private bool CanPerformAttack(float requiredMana, float requiredStamina, float availableMana,
        float availableStamina)
    {
        //TODO Stamina system
        // if (availableStamina < requiredStamina)
        // {
        // return false;
        // }

        
        //MANA SYSTEM DISABLED FOR NOW
        // if (!_playerMana.TryUseMana(requiredMana))
        // {
        //     return false;
        // }

        _playerStamina.UseStamina(requiredStamina);
        return true;
    }

    private void CreateSlashEffect(SlashEffect slash)
    {
        slash.weapon = _weapon;
        PlayWeaponSound(slash.weapon.AttackSoundType);
        slash.SetParent(firePoint);
        attackCooldown = slash.weapon.timeBetweenAttack;
        StartCoroutine(AttackCooldown());
    }

    private void PlayWeaponSound(SoundType soundType)
    {
        SoundManager.Instance.PlaySound(soundType);
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        var elapsed = 0f;

        cooldownImage.fillAmount = 0f;

        var cooldown = GetCurrentAttackCooldown();
        
        while (elapsed < cooldown)
        {
            elapsed += Time.deltaTime;
            cooldownImage.fillAmount = Mathf.Clamp01(elapsed / cooldown);
            yield return null;
        }

        cooldownImage.fillAmount = 1f;
        canAttack = true;
    }
    
    public float GetCurrentAttackCooldown()
    {
        var cooldownReduction =
            _playerStats.GetBonusValue(
                BonusType.AttackCooldownReduction);        
        
        return Mathf.Max(
            0.1f,
            attackCooldown *
            _attackCooldownMultiplier *
            (1f - cooldownReduction)
        );
    }
    
    public void ApplyAttackCooldownMultiplier(
        float multiplier,
        float duration)
    {
        StartCoroutine(
            AttackCooldownMultiplierCoroutine(
                multiplier,
                duration));
    }

    private IEnumerator AttackCooldownMultiplierCoroutine(
        float multiplier,
        float duration)
    {
        _attackCooldownMultiplier *= multiplier;

        yield return new WaitForSeconds(duration);

        _attackCooldownMultiplier /= multiplier;
    }

    public void EquipWeapon(WeaponItemSO newItemWeapon)
    {
        _weapon = newItemWeapon;
        SetPlayerTotalDamage();
    }

    private float SetPlayerTotalDamage()
    {
        var baseDamage = _playerStats.BaseDamage;

        var weaponDamage = 0f;

        if (_weapon != null)
        {
            weaponDamage = _weapon.Damage;

            var equippedWeapon =
                EquippedItemsManager.Instance.EquippedItems[0];

            var bonusDamage =
                ItemParameterHelper.GetParameterValue(
                    equippedWeapon.itemState,
                    damageBonusParameter);

            weaponDamage += bonusDamage;
        }

        var damage = baseDamage + weaponDamage;

        damage *= _playerStats.GetDamageBonusMultiplier();
        damage *= _currentDmgMultiplier;

        _playerStats.TotalDamage = damage;

        return damage;
    }

    public float GetPlayerDamage()
    {
        var critCheck = RNGManager.Instance.GetRandomInt();
        var critStat = Mathf.Round(_playerStats.GetCritChanceBonusMultiplier() * 100f - 100f);
        
        var baseDamage = _playerStats.TotalDamage;

        var minDamage = baseDamage * (1f - damageVariancePercent);
        var maxDamage = baseDamage * (1f + damageVariancePercent);

        var finalDamage = RNGManager.Instance.GetRandomFloat(minDamage, maxDamage);

        
        if (critCheck <= critStat)
            finalDamage *= _playerStats.GetCritMultiplier();
        
        return Mathf.Round(finalDamage);
    }

    public void ApplyDmgMultiplier(float multiplier, float duration)
    {
        StartCoroutine(DmgMultiplierCoroutine(multiplier, duration));
    }

    private IEnumerator DmgMultiplierCoroutine(float multiplier, float duration)
    {
        _currentDmgMultiplier *= multiplier;
        SetPlayerTotalDamage();

        yield return new WaitForSeconds(duration);

        _currentDmgMultiplier /= multiplier;
        SetPlayerTotalDamage();
    }
    
    private void OnEnable()
    {
        _playerInput.actions["Attack"].performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        _playerInput.actions["Attack"].performed -= OnAttackPerformed;
    }
    
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        Attack();
    }
    
    public void RecalculateDamage()
    {
        var totalDmg = SetPlayerTotalDamage();
        PlayerStatisticsManager.Instance.SetDamage(totalDmg);
    }
}
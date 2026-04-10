using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [FormerlySerializedAs("weapon")] [SerializeField]
    private WeaponItemSO _weapon;

    [SerializeField] private PlayerStatsSO _playerStats;
    [SerializeField] private Image cooldownImage;

    public Transform firePoint;
    public GameObject slashEffect;
    private float attackCooldown;
    private bool canAttack = true;
    private PlayerMana _playerMana;
    private PlayerStamina _playerStamina;
    private PlayerInput _playerInput;
    private SlashEffect _slash;

    private float _currentDmgMultiplier = 1f;

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
            GameManager.Instance.CurrentMode == GameMode.WorldMap) return;

        var manaCost = _weapon.ProjectilePrefab ? _weapon.ProjectilePrefab.ProjectileSo.ManaCost : 0f;

        var canPerformAttack = CanPerformAttack(manaCost, _weapon.RequiredStamina, _playerMana.CurrentMana,
            _playerStamina.CurrentStamina);
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

        if (requiredMana > 0 && availableMana <= 0)
        {
            return false;
        }

        _playerMana.UseMana(requiredMana);
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

    private void PlayWeaponSound(SoundType soundType, float volume = 1f)
    {
        SoundManager.Instance.PlaySound(soundType, volume);
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        var elapsed = 0f;

        cooldownImage.fillAmount = 0f;
        while (elapsed < attackCooldown)
        {
            elapsed += Time.deltaTime;
            cooldownImage.fillAmount = Mathf.Clamp01(elapsed / attackCooldown);
            yield return null;
        }

        cooldownImage.fillAmount = 1f;
        canAttack = true;
    }

    public void EquipWeapon(WeaponItemSO newItemWeapon)
    {
        _weapon = newItemWeapon;
        SetPlayerTotalDamage();
    }

    private float SetPlayerTotalDamage()
    {
        var baseDamage = _playerStats.BaseDamage;
        var weaponDamage = _weapon != null ? _weapon.Damage : 0f;
        var npcBonus = _playerStats.BonusDamage;

        var final = (baseDamage + weaponDamage + npcBonus) * _currentDmgMultiplier;

        _playerStats.TotalDamage = final;
        
        Debug.Log(final);
        
        return final;
    }

    public float GetPlayerDamage => _playerStats.TotalDamage;

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
        _playerInput.actions["Attack"].performed += _ => Attack();
    }

    private void OnDisable()
    {
        _playerInput.actions["Attack"].performed -= _ => Attack();
    }
    
    public void RecalculateDamage()
    {
        var totalDmg = SetPlayerTotalDamage();
        PlayerStatisticsManager.Instance.SetDamage(totalDmg);
    }
}
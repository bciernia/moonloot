using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [FormerlySerializedAs("weapon")] [SerializeField] private WeaponItemSO _weapon;
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

    private void Attack()
    {
        if (!canAttack || DialogueManager.Instance.IsInDialogue()) return;

        var manaCost = _weapon.ProjectilePrefab ? _weapon.ProjectilePrefab.ProjectileSo.ManaCost : 0f;
        
        var canPerformAttack = CanPerformAttack(manaCost, _weapon.RequiredStamina, _playerMana.CurrentMana, _playerStamina.CurrentStamina);
        if (!canPerformAttack) return;
        
        var slashObject = Instantiate(slashEffect, firePoint.position, firePoint.rotation);
        var slash = slashObject.GetComponent<SlashEffect>();
        slash.SetShooter(gameObject);
        
        CreateSlashEffect(slash);
    }

    private bool CanPerformAttack(float requiredMana, float requiredStamina, float availableMana, float availableStamina)
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
        slash.SetParent(firePoint);
        attackCooldown = slash.weapon.timeBetweenAttack;
        StartCoroutine(AttackCooldown());
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
        _playerStats.TotalDamage = _playerStats.BaseDamage + _weapon.Damage;
    }
    
    private void OnEnable()
    {
        _playerInput.actions["Attack"].performed += _ => Attack();
    }

    private void OnDisable()
    {
        _playerInput.actions["Attack"].performed -= _ => Attack();
    }
}
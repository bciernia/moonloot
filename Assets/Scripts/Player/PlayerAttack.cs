using System;
using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private WeaponSO weapon;
    
    public Transform firePoint;
    public GameObject slashEffect;
    private float attackCooldown;
    private bool canAttack = true;
    private PlayerMana _playerMana;
    private PlayerStamina _playerStamina;
    public TextMeshProUGUI _weaponName;
    
    private SlashEffect _slash;

    private void Awake()
    {
        _playerMana = GetComponent<PlayerMana>();
        _playerStamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }
        //TODO Do wywalenia/przeniesienia
        _weaponName.text = $"Weapon: {weapon.name.Split("_")[1]}";
    }

    private void Attack()
    {
        var canPerformAttack = CanPerformAttack(weapon.RequiredMana, weapon.RequiredStamina, _playerMana.CurrentMana, _playerStamina.CurrentStamina);
        if (!canPerformAttack) return;
        
        var slashObject = Instantiate(slashEffect, firePoint.position, firePoint.rotation);
        var slash = slashObject.GetComponent<SlashEffect>();
        slash.SetShooter(gameObject);
        
        CreateSlashEffect(slash);
    }

    private bool CanPerformAttack(float requiredMana, float requiredStamina, float availableMana, float availableStamina)
    {
        if (availableStamina < requiredStamina)
        {
            return false;
        }
        
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
        slash.weapon = weapon;
        slash.SetParent(firePoint);
        attackCooldown = slash.weapon.timeBetweenAttack;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false; 
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
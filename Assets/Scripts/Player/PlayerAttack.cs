using System;
using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private WeaponSO weapon;
    
    public Transform firePoint;
    public GameObject slashEffect;
    private float attackCooldown;
    private bool canAttack = true;
    private PlayerMana playerMana;
    
    private SlashEffect _slash;

    private void Awake()
    {
        playerMana = GetComponent<PlayerMana>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        var canPerformAttack = CanPerformAttack(weapon.RequiredMana, playerMana.CurrentMana);
        if (!canPerformAttack) return;
        
        var slashObject = Instantiate(slashEffect, firePoint.position, firePoint.rotation);
        var slash = slashObject.GetComponent<SlashEffect>();
        slash.SetShooter(gameObject);
        
        CreateSlashEffect(slash);
    }

    private bool CanPerformAttack(float requiredMana, float availableMana)
    {
        if (requiredMana == 0)
        {
            return true;
        }
        
        if (availableMana >= requiredMana)
        {
            playerMana.UseMana(requiredMana);
            return true;
        }

        return false;
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
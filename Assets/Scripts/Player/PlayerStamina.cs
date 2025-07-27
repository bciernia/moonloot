using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStamina : MonoBehaviour
{
    private Player player;
    private PlayerMovement playerMovement;

    public float CurrentStamina { get; set; }

    private float timer = 0f;
    private const float delayAmount = 1f;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        ResetStamina();
    }

    private void Update()
    {
        // RecoverStamina();
    }

    private void RecoverStamina()
    {
        if (!(CurrentStamina < player.PlayerStats.MaxStamina)) return;

        // if (playerMovement.IsSprinting) return;
        
        timer += Time.deltaTime;
        if (!(timer >= delayAmount)) return;
        
        timer = 0f;
        player.PlayerStats.Stamina++;
        CurrentStamina = player.PlayerStats.Stamina;
    }

    //TODO uzyte po wypiciu potki
    public void RecoverStamina(float amount)
    {
        player.PlayerStats.Stamina += amount;
        player.PlayerStats.Stamina = Mathf.Min(player.PlayerStats.Stamina, player.PlayerStats.MaxStamina);
        CurrentStamina = player.PlayerStats.Stamina;
    }
    
    //TODO otrzymanie obrażeń jakiegoś zaklęcia czy stanu
    private void DecreaseStamina()
    {
        timer += Time.deltaTime;
        if (!(timer >= delayAmount)) return;     
        
        if (CurrentStamina > 0)
        {
            timer = 0f;
            CurrentStamina -= 1f;
            player.PlayerStats.Stamina -= 1f;
        }
    }
    
    public void UseStamina(float amount)
    {
        if (CurrentStamina > 0)
        {
            player.PlayerStats.Stamina = Mathf.Max(player.PlayerStats.Stamina - amount, 0f);
            CurrentStamina = player.PlayerStats.Stamina;
        }
    }

    public bool CanRestoreStamina()
    {
        return player.PlayerStats.Stamina < player.PlayerStats.MaxStamina;
    }

    private void ResetStamina()
    {
        player.PlayerStats.Stamina = player.PlayerStats.MaxStamina;
        CurrentStamina = player.PlayerStats.MaxStamina;
    }
}
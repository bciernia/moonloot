using System.Collections.Generic;
using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    public DifficultyLevel DifficultyLevel { get; set; }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ArenaManager.Instance.StartArena(DifficultyLevel);
        
        Destroy(gameObject);
    }
    
}
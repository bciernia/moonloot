using System.Collections.Generic;
using UnityEngine;

public class AppearOnProximity : MonoBehaviour
{
    private EnemyBrain _enemyBrain;

    private readonly List<GameObject> _goList = new List<GameObject>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            _goList.Add(child.gameObject);
        }
        
        foreach (var go in _goList)
        {
            go.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var go in _goList)
            {
                go.SetActive(true);
            }
        }
    }
}

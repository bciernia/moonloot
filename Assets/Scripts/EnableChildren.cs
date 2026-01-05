using System;
using UnityEngine;

public class EnableChildren : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
            
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}

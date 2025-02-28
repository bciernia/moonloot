using System;
using TMPro;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance;
    
    [SerializeField] private DamageText damageTextPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamageText(float damageAmount, Transform parent)
    {
        var damageText = Instantiate(damageTextPrefab, parent);
        damageText.transform.position += Vector3.right * 0.5f;
        damageText.SetDamageText(damageAmount);
    }
}
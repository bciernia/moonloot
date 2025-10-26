using System;
using TMPro;
using UnityEngine;

public class DamageManager : Singleton<DamageManager>
{
    [SerializeField] private DamageText damageTextPrefab;

    public void ShowDamageText(float damageAmount, Transform parent)
    {
        var damageText = Instantiate(damageTextPrefab, parent);
        damageText.SetDamageText(damageAmount);
    }
}
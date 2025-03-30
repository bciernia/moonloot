using System;
using TMPro;
using UnityEngine;

public class DamageManager : Singleton<DamageManager>
{
    [SerializeField] private DamageText damageTextPrefab;

    public void ShowDamageText(float damageAmount, Transform parent)
    {
        var damageText = Instantiate(damageTextPrefab, parent);
        damageText.transform.position += Vector3.right * 0.5f;
        damageText.SetDamageText(damageAmount);
    }
}
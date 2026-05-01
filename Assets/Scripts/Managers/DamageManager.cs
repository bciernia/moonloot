using System;
using TMPro;
using UnityEngine;

public class FloatingTextManager : Singleton<FloatingTextManager>
{
    [SerializeField] private FloatingText textPrefab;

    public void ShowDamageText(float damageAmount, Transform parent)
    {
        var floatingText = Instantiate(textPrefab, parent);
        floatingText.SetDamageText(damageAmount);
    }
    
    public void ShowHealText(float damageAmount, Transform parent)
    {
        var floatingText = Instantiate(textPrefab, parent);
        floatingText.SetHealText(damageAmount);
    }
    
    public void ShowText(string text, Transform parent)
    {
        var floatingText = Instantiate(textPrefab, parent);
        floatingText.SetFloatingText(text);
    }
    
    public void ShowWarningText(string text, Transform parent)
    {
        var floatingText = Instantiate(textPrefab, parent);
        floatingText.SetFloatingWarningText(text);
    }
}
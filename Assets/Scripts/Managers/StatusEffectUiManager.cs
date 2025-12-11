using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUIManager : Singleton<StatusEffectUIManager>
{
    [SerializeField] private GameObject effectPrefab;   
    [SerializeField] private Transform effectsContainer;

    private readonly Dictionary<string, GameObject> activeEffectIcons = new();

    public GameObject CreateEffectUI(Effect effect)
    {
        if (activeEffectIcons.TryGetValue(effect.Name, out var existing))
        {
            return existing;
        }
        
        var instance = Instantiate(effectPrefab, effectsContainer);

        var tooltip = instance.GetComponent<TooltipTrigger>();
        var image = instance.GetComponent<Image>();

        tooltip.header = effect.Name;
        tooltip.content = effect.Description;
        image.sprite = effect.Icon;
        
        activeEffectIcons[effect.Name] = instance;

        return instance;
    }
    
    
    public void RemoveEffectUI(string effectName)
    {
        if (activeEffectIcons.TryGetValue(effectName, out var icon))
        {
            Destroy(icon);
            activeEffectIcons.Remove(effectName);
        }
    }
}
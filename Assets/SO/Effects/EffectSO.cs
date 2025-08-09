using UnityEngine;

public class EffectSO : ScriptableObject
{
    public string Name { get; set; }
    [TextArea]
    public string Description { get; set; }
    public Sprite Image { get; set; }
}

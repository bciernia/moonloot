using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NightLocation", menuName = "Horde/NightLocation")]
public class NightLocationSO : ScriptableObject
{
    [Header("Visuals")] public string Title;

    [TextArea(3, 6)] public string Description;

    public Sprite PreviewImage;

    [Header("Night Type")] public NightLocationType NightType;

    [Header("Scene")] public string SceneName;

    public bool IsBossArena;
    

// public SceneReference Scene;
    // public AudioClip Music;
    public Color AmbientColor;
    public HordeMutation ForcedMutation;
    
    public float DifficultyModifier = 1;
    // public List<LootTable> ExtraLoot;
}
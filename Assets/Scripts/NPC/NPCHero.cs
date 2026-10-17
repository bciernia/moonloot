using UnityEngine;

[CreateAssetMenu(fileName = "NPCHero_", menuName = "Game/NPC/Hero")]
public class NPCHero : VillageNpcData
{
    [Tooltip("Stable ID used to save this hero's unlock and progression.")]
    public string HeroId;

    public string StableId => string.IsNullOrWhiteSpace(HeroId) ? name : HeroId.Trim();

    public float Strength;
    public float MaxHp;
    public float Hp;

    [TextArea(2, 4)]
    public string Pros;

    [TextArea(2, 4)]
    public string Cons;

    [Tooltip("Outfit asset whose animator controller is used when this hero is selected.")]
    public OutfitItemSO Outfit;

    [TextArea(2,4)]
    public string MenuDescription;
}

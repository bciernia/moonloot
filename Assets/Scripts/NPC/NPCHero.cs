using UnityEngine;
using System.Collections.Generic;

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

    [Tooltip("Weapon equipped when this hero starts a new game.")]
    public WeaponItemSO StartingWeapon;

    [TextArea(2,4)]
    public string MenuDescription;

    [Header("Start of run dialogue")]
    [Tooltip("Optional pool of lines used when this hero enters START_MAP. Leave empty to use the built-in class-specific lines.")]
    [TextArea(2, 3)]
    public List<string> StartRunQuotes = new();
}

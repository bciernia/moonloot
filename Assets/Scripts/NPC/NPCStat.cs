using UnityEngine;

[CreateAssetMenu(fileName = "NPCStat_", menuName = "Game/NPC/Stat")]
public class NPCStat : NPCData
{
    public float? BonusDmg;
    public float? BonusMoveSpeed;
    public float? BonusCrit;
}
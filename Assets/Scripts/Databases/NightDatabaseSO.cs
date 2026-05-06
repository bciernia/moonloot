using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NightDatabase", menuName = "Horde/NightDatabase")]
public class NightDatabaseSO : ScriptableObject
{
    public List<NightLocationSO> NormalNights;
    public List<NightLocationSO> HeroNights;
}
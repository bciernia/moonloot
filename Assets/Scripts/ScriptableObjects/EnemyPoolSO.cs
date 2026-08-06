using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pools/Enemy Pool")]
public class EnemyPoolSO : ScriptableObject
{
    public List<GameObject> NormalEnemies;
    public List<GameObject> EliteEnemies;
    public List<GameObject> BossEnemies;
}
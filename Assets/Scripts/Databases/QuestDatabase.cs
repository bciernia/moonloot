using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestDatabase : Singleton<QuestDatabase>
{
    [SerializeField] private List<QuestSO> quests;

    private static Dictionary<string, QuestSO> _quests;

    protected override void Awake()
    {
        _quests = quests.ToDictionary(q => q.Id, q => q);
    }

    public static QuestSO Get(string id) => _quests[id];
}
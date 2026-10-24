using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestDatabase : Singleton<QuestDatabase>
{
    [SerializeField] private List<QuestSO> quests;
    [SerializeField] private List<QuestSO> rescuedNpcQuestPool = new List<QuestSO>();

    private static Dictionary<string, QuestSO> _quests;
    private static List<QuestSO> _rescuedNpcQuestPool;

    protected override void Awake()
    {
        base.Awake();
        _rescuedNpcQuestPool = rescuedNpcQuestPool == null
            ? new List<QuestSO>()
            : rescuedNpcQuestPool.Where(q => q != null).Distinct().ToList();

        var allQuests = (quests ?? new List<QuestSO>())
            .Concat(_rescuedNpcQuestPool)
            .Where(q => q != null);

        _quests = allQuests
            .GroupBy(q => q.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public static IReadOnlyList<QuestSO> GetRescuedNpcQuestPool()
    {
        if (_rescuedNpcQuestPool == null)
            return new List<QuestSO>();

        return _rescuedNpcQuestPool;
    }

    public static QuestSO Get(string id)
    {
        if (_quests != null && !string.IsNullOrWhiteSpace(id) && _quests.TryGetValue(id, out var quest))
            return quest;

        return null;
    }

}

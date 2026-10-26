using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private List<QuestSO> _quests = new List<QuestSO>();
    [SerializeField] private string _giverID;
    [SerializeField] private string _giverDisplayName;
    [SerializeField] private string _giverProfession;
    [SerializeField] private Sprite _giverPortrait;
    private VillageNpcRuntime _runtimeNpc;
    private QuestSO _assignedQuest;

    private QuestJournal _questJournal;

    public QuestSO AssignedQuest => _assignedQuest;

    public string GiverID => _runtimeNpc != null
        ? _runtimeNpc.RuntimeID
        : string.IsNullOrWhiteSpace(_giverID) ? GetFallbackGiverName() : _giverID.Trim();

    public string GiverDisplayName => _runtimeNpc != null
        ? !string.IsNullOrWhiteSpace(_runtimeNpc.QuestGiverName)
            ? _runtimeNpc.QuestGiverName
            : _runtimeNpc.Name
        : string.IsNullOrWhiteSpace(_giverDisplayName) ? GetFallbackGiverName() : _giverDisplayName.Trim();

    public string GiverProfession => _runtimeNpc != null
        ? !string.IsNullOrWhiteSpace(_runtimeNpc.QuestGiverProfession)
            ? _runtimeNpc.QuestGiverProfession
            : _runtimeNpc.Profession
        : !string.IsNullOrWhiteSpace(_giverProfession)
            ? _giverProfession.Trim()
            : (GetComponent<NPCStatUpgrade>() ?? GetComponentInParent<NPCStatUpgrade>() ??
               GetComponentInChildren<NPCStatUpgrade>(true))?.GetNpcProfession() ?? string.Empty;

    public Sprite GiverPortrait => _giverPortrait != null
        ? _giverPortrait
        : _runtimeNpc?.Data?.Portrait ??
          GetComponent<NPCStatUpgrade>()?.GetNpcPortrait() ??
          GetComponentInChildren<SpriteRenderer>(true)?.sprite;

    private void Awake()
    {
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
    }

    public QuestSO GetQuest(int questIndex = 0)
    {
        if (_assignedQuest != null)
            return questIndex == 0 ? _assignedQuest : null;

        return IsValidQuestIndex(questIndex) ? _quests[questIndex] : null;
    }

    public string GetGiverDisplayName() => GiverDisplayName;

    public string GetQuestStory(int questIndex = 0)
    {
        var quest = GetQuest(questIndex);
        if (quest == null)
            return string.Empty;

        var status = _questJournal != null ? _questJournal.GetQuestStatus(quest) : null;
        if (status == null ||
            (!string.IsNullOrWhiteSpace(status.GiverID) && status.GiverID != GiverID))
        {
            return quest.GetQuestStory();
        }

        if (status.IsQuestCompleted)
            return quest.GetRandomCompletedDialogueLine();

        if (status.IsReadyToTurnIn)
            return $"You have completed {quest.GetQuestTitle()}. Let me give you your reward.";

        return quest.GetActiveQuestDialogue();
    }

    public string GetQuestTurnInDialogue(int questIndex = 0)
    {
        var quest = GetQuest(questIndex);
        var status = quest != null && _questJournal != null
            ? _questJournal.GetQuestStatus(quest)
            : null;

        return status != null && status.IsQuestCompleted
            ? quest.GetTurnInDialogue()
            : string.Empty;
    }

    public List<QuestSO> GetQuests() => _assignedQuest != null
        ? new List<QuestSO> { _assignedQuest }
        : _quests ?? new List<QuestSO>();

    public void SetRuntimeNpc(VillageNpcRuntime runtime)
    {
        _runtimeNpc = runtime;
        _assignedQuest = runtime != null && !string.IsNullOrWhiteSpace(runtime.AssignedQuestId)
            ? QuestDatabase.Get(runtime.AssignedQuestId)
            : null;

        var questService = GetComponent<QuestService>() ??
                           GetComponentInParent<QuestService>() ??
                           GetComponentInChildren<QuestService>(true);
        questService?.RefreshQuestList();

        if (runtime != null && !string.IsNullOrWhiteSpace(runtime.AssignedQuestId) && _assignedQuest == null)
        {
            Debug.LogWarning(
                $"Quest '{runtime.AssignedQuestId}' assigned to NPC '{runtime.Name}' could not be found in QuestDatabase.",
                this);
        }
    }

    public void GiveQuest(int id, string entryKey = "")
    {
        var quest = GetQuest(id);
        if (quest == null || _questJournal == null)
            return;

        _questJournal.AddQuest(quest, entryKey, GiverID, GiverDisplayName, GiverPortrait, GiverProfession);
    }
    
    public bool HasPlayerQuestSOInQuestList(int id = 0) =>
        GetQuest(id) != null && _questJournal != null && _questJournal.HasPlayerQuest(GetQuest(id));

    // Expose these methods to EasyTalk dialogue condition/action nodes.
    public bool CanTurnInQuest(int id = 0) =>
        GetQuest(id) != null && _questJournal != null &&
        _questJournal.CanTurnInQuest(GetQuest(id), GiverID);

    public bool TurnInQuest(int id = 0) =>
        GetQuest(id) != null && _questJournal != null &&
        _questJournal.TurnInQuest(GetQuest(id), GiverID);

    private bool IsValidQuestIndex(int index) =>
        _quests != null && index >= 0 && index < _quests.Count && _quests[index] != null;

    private string GetFallbackGiverName()
    {
        var npcName = GetComponent<NPCStatUpgrade>()?.GetNpcName();
        return string.IsNullOrWhiteSpace(npcName) ? gameObject.name : npcName;
    }
}

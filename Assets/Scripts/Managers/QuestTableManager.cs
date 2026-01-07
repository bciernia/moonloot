using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestTableManager : Singleton<QuestTableManager>
{
    [SerializeField] private List<GameObject> QuestPlacesOnBoard = new List<GameObject>();
    [SerializeField] private GameObject QuestCardPrefab;
    [SerializeField] private GameObject QuestDescriptionPanel;
    [SerializeField] private TextMeshProUGUI QuestTitle;
    [SerializeField] private TextMeshProUGUI QuestDescription;
    [SerializeField] private TextMeshProUGUI QuestReward;
    [SerializeField] private Button QuestButton;
    [SerializeField] private GameObject QuestTable;

    private QuestJournal PlayerQuests;
    
    private QuestSO ChosenQuest { get; set; }
    
    private readonly List<GameObject> spawnedQuestCards = new List<GameObject>();

    private const string QUEST_TAKEN_FROM_TABLE = "QUEST_TAKEN_FROM_TABLE";

    protected override void Awake()
    {
        base.Awake();
        PlayerQuests = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
    }

    public void PrepareQuestTable(List<QuestSO> questList)
    {
        ChosenQuest = null;
        QuestDescriptionPanel.SetActive(false);
        foreach(var card in spawnedQuestCards)
            Destroy(card);
        spawnedQuestCards.Clear();

        var count = Mathf.Min(questList.Count, QuestPlacesOnBoard.Count);

        for (var i = 0; i < count; i++)
        {
            if (PlayerQuests.HasPlayerQuest(questList[i])) continue;
            
            var cardGO = Instantiate(QuestCardPrefab, QuestPlacesOnBoard[i].transform);
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;

            spawnedQuestCards.Add(cardGO);

            var questCard = cardGO.GetComponent<QuestCard>();
            questCard.Setup(questList[i], OnQuestCardClicked);
        }
        
        QuestTable.SetActive(true);
        PauseManager.Instance.RequestPause();
    }

    private void OnQuestCardClicked(QuestSO quest)
    {
        QuestDescriptionPanel.SetActive(true);
        ChosenQuest = quest;
        QuestTitle.text = quest.GetQuestTitle();
        QuestDescription.text = quest.GetQuestDescription();
        QuestReward.text = PrepareRewardForQuest(quest.GetRewards().ToList());
        QuestButton.onClick.RemoveAllListeners();
        QuestButton.onClick.AddListener(() => GetTask(quest));
    }

    private string PrepareRewardForQuest(List<QuestSO.Reward> rewards)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Reward: ");
        foreach (var reward in rewards)
        {
            sb.AppendLine($"{reward.item.item.name} x{reward.item.quantity}");
        }

        return sb.ToString();
    }

    private void GetTask(QuestSO quest)
    {
        PlayerQuests.AddQuest(quest, QUEST_TAKEN_FROM_TABLE);
        RemoveQuestCard(quest);
        QuestDescriptionPanel.SetActive(false);
    }
    
    private void RemoveQuestCard(QuestSO quest)
    {
        for (var i = 0; i < spawnedQuestCards.Count; i++)
        {
            var questCard = spawnedQuestCards[i].GetComponent<QuestCard>();
            if (questCard != null && questCard.HasQuest(quest))
            {
                Destroy(spawnedQuestCards[i]);
                spawnedQuestCards.RemoveAt(i);
                break;
            }
        }
    }
}
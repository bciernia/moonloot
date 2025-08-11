using System;
using System.Collections.Generic;
using TMPro;
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

    private QuestList PlayerQuests;
    
    private Quest ChosenQuest { get; set; }
    
    private readonly List<GameObject> spawnedQuestCards = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        PlayerQuests = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
    }

    public void PrepareQuestTable(List<Quest> questList)
    {
        ChosenQuest = null;
        QuestDescriptionPanel.SetActive(false);
        foreach(var card in spawnedQuestCards)
            Destroy(card);
        spawnedQuestCards.Clear();

        var count = Mathf.Min(questList.Count, QuestPlacesOnBoard.Count);

        for (var i = 0; i < count; i++)
        {
            if (PlayerQuests.IsQuestInQuestList(questList[i])) continue;
            
            var cardGO = Instantiate(QuestCardPrefab, QuestPlacesOnBoard[i].transform);
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;

            spawnedQuestCards.Add(cardGO);

            var questCard = cardGO.GetComponent<QuestCard>();
            questCard.Setup(questList[i], OnQuestCardClicked);
        }
    }

    private void OnQuestCardClicked(Quest quest)
    {
        QuestDescriptionPanel.SetActive(true);
        ChosenQuest = quest;
        QuestTitle.text = quest.GetTitle();
        QuestDescription.text = quest.GetDescription();
        QuestReward.text = $"Reward:\n{quest.GetRewardDescription()}";
        QuestButton.onClick.RemoveAllListeners();
        QuestButton.onClick.AddListener(() => GetTask(quest));
    }

    private void GetTask(Quest quest)
    {
        var questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.AddQuest(quest);
        RemoveQuestCard(quest);
        QuestDescriptionPanel.SetActive(false);
    }
    
    private void RemoveQuestCard(Quest quest)
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
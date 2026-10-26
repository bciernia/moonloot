using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.NewInventory.Model;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NPCDialogueBubble : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Dialogues")]
    [SerializeField] private TextAsset _dialogueFile;

    [Header("Timing")]
    [SerializeField] private float _minDelay = 5f;
    [SerializeField] private float _maxDelay = 15f;
    [SerializeField] private float _displayTime = 3f;

    private Dictionary<string, List<string>> _dialogueGroups = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _remainingLines = new();

    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);

        LoadDialogues();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Base_Demo" || _dialogueGroups.Count == 0)
            return;

        StartCoroutine(DialogueRoutine());
    }

    private void LoadDialogues()
    {
        if (_dialogueFile == null)
        {
            Debug.LogWarning($"No dialogue file assigned for {gameObject.name}");
            return;
        }

        try
        {
            var dialogues = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_dialogueFile.text);
            if (dialogues != null)
                _dialogueGroups = new Dictionary<string, List<string>>(dialogues, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load NPC dialogue file for {gameObject.name}: {exception.Message}", this);
        }
    }

    private IEnumerator DialogueRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minDelay, _maxDelay));
            ShowContextualDialogue();
            yield return new WaitForSeconds(_displayTime);
            HideDialogue();
        }
    }

    private void ShowContextualDialogue()
    {
        if (_panel == null || _text == null || _dialogueGroups.Count == 0)
            return;

        var history = HordeManager.Instance;
        string groupKey = null;
        var dialogueEvent = history != null ? history.GetNextPendingDialogueEvent() : null;
        var hasNewEvent = dialogueEvent != null &&
                          TryGetEventGroup(dialogueEvent.EventName, out groupKey) &&
                          history.TryConsumeDialogueEvent(dialogueEvent.Sequence);

        if (!hasNewEvent)
            groupKey = SelectAmbientGroup(history);

        if (string.IsNullOrEmpty(groupKey) || !TryGetNextLine(groupKey, out var line))
            return;

        _text.text = ApplyPlaceholders(line, history, dialogueEvent);
        _panel.SetActive(true);
    }

    private bool TryGetEventGroup(string eventName, out string groupKey)
    {
        groupKey = eventName switch
        {
            "NpcRescued" => "Event:NpcRescued",
            "QuestCompleted" => "Event:QuestCompleted",
            "HordeWon" => "Event:HordeWon",
            "HordeLost" => "Event:HordeLost",
            _ => string.Empty
        };

        return !string.IsNullOrEmpty(groupKey) &&
               _dialogueGroups.TryGetValue(groupKey, out var lines) && lines != null && lines.Count > 0;
    }

    private string SelectAmbientGroup(HordeManager history)
    {
        var candidates = new List<string>();
        AddIfAvailable(candidates, GetLocationGroup(history != null ? history.LastDialogueLocation : null));
        AddIfAvailable(candidates, GetWeaponGroup());
        AddIfAvailable(candidates, GetNightGroup(history));
        AddIfAvailable(candidates, "Default");

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    private string GetLocationGroup(string sceneName)
    {
        return string.IsNullOrWhiteSpace(sceneName) ? null : $"Location:{sceneName.Trim()}";
    }

    private string GetNightGroup(HordeManager history)
    {
        if (history == null)
            return null;

        var specificKey = $"Night:{history.currentHorde}";
        if (_dialogueGroups.ContainsKey(specificKey))
            return specificKey;

        return history.currentHorde >= 6 ? "Night:Late" : "Night:Default";
    }

    private static string GetWeaponGroup()
    {
        var equipped = EquippedItemsManager.Instance?.EquippedItems;
        if (equipped == null || equipped.Count == 0 || equipped[0].item is not WeaponItemSO weapon)
            return null;

        if (weapon.WeaponType == WeaponType.Bow)
            return "Weapon:Bow";

        if (weapon.WeaponType is WeaponType.FireSpell or WeaponType.EarthSpell or WeaponType.IceSpell)
            return "Weapon:Magic";

        return weapon.ProjectilePrefab != null ? "Weapon:Ranged" : "Weapon:Melee";
    }

    private void AddIfAvailable(List<string> candidates, string key)
    {
        if (!string.IsNullOrWhiteSpace(key) && _dialogueGroups.TryGetValue(key, out var lines) && lines != null && lines.Count > 0)
            candidates.Add(key);
    }

    private bool TryGetNextLine(string groupKey, out string line)
    {
        line = null;
        if (!_dialogueGroups.TryGetValue(groupKey, out var sourceLines) || sourceLines == null || sourceLines.Count == 0)
            return false;

        if (!_remainingLines.TryGetValue(groupKey, out var availableLines) || availableLines.Count == 0)
        {
            availableLines = new List<string>(sourceLines);
            _remainingLines[groupKey] = availableLines;
        }

        var index = Random.Range(0, availableLines.Count);
        line = availableLines[index];
        availableLines.RemoveAt(index);
        return !string.IsNullOrWhiteSpace(line);
    }

    private static string ApplyPlaceholders(
        string line,
        HordeManager history,
        NpcDialogueHistoryEntry dialogueEvent)
    {
        if (history == null || string.IsNullOrEmpty(line))
            return line;

        var night = (dialogueEvent?.EventName == "HordeWon" || dialogueEvent?.EventName == "HordeLost") &&
                    !string.IsNullOrWhiteSpace(dialogueEvent.Detail)
            ? dialogueEvent.Detail
            : history.currentHorde.ToString();

        var eventSubject = dialogueEvent != null ? dialogueEvent.Subject : history.LastDialogueEventSubject;
        var eventDetail = dialogueEvent != null ? dialogueEvent.Detail : history.LastDialogueEventDetail;

        return line
            .Replace("{Map}", string.IsNullOrWhiteSpace(history.LastDialogueLocation) ? "the last expedition" : history.LastDialogueLocation)
            .Replace("{Night}", night)
            .Replace("{Npc}", string.IsNullOrWhiteSpace(eventSubject) ? "someone" : eventSubject)
            .Replace("{Quest}", string.IsNullOrWhiteSpace(eventSubject) ? "that quest" : eventSubject)
            .Replace("{Giver}", string.IsNullOrWhiteSpace(eventDetail) ? "the quest giver" : eventDetail);
    }

    private void HideDialogue()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }
}

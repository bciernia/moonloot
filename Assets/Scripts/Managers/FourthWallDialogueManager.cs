using System.Collections.Generic;
using EasyTalk.Controller;
using UnityEngine;

public class FourthWallDialogueManager : MonoBehaviour
{
    [Header("Ustawienia")]
    public List<string> fourthWallLines;           // Lista tekstów czwartej ściany
    public float minDelay = 60f;                   // Minimalny czas do kolejnej próby (sekundy)
    public float maxDelay = 180f;                  // Maksymalny czas do kolejnej próby (sekundy)
    [Range(0f, 1f)]
    public float sceneChangeChance = 0.3f;         // Szansa po zmianie sceny
    [Range(0f, 1f)]
    public float afterDialogueChance = 0.2f;       // Szansa po zakończeniu zwykłego dialogu
    [Range(0f, 1f)]
    public float randomIdleChance = 0.5f;          // Szansa przy losowym sprawdzeniu

    private bool fourthWallDialogueInProgress = false;
    private DialogueController playerDialogueController;

    void Start()
    {
        playerDialogueController = GameObject.FindWithTag("Player")?.GetComponent<DialogueController>();
        ScheduleNextDialogueAttempt();
    }

    void ScheduleNextDialogueAttempt()
    {
        float delay = Random.Range(minDelay, maxDelay);
        Invoke(nameof(TryStartFourthWallDialogue), delay);
    }

    void TryStartFourthWallDialogue()
    {
        if (!IsDialogueActive() && Random.value < randomIdleChance)
        {
            TriggerDialogue();
        }

        // Zawsze zaplanuj kolejne sprawdzenie
        ScheduleNextDialogueAttempt();
    }

    public void OnSceneChanged()
    {
        if (!IsDialogueActive() && Random.value < sceneChangeChance)
        {
            TriggerDialogue();
        }
    }

    public void OnDialogueEnded()
    {
        if (!IsDialogueActive() && Random.value < afterDialogueChance)
        {
            TriggerDialogue();
        }
    }

    void TriggerDialogue()
    {
        if (IsDialogueActive()) return;

        if (fourthWallLines == null || fourthWallLines.Count == 0)
        {
            Debug.LogWarning("Brak tekstów czwartej ściany!");
            return;
        }

        string line = fourthWallLines[Random.Range(0, fourthWallLines.Count)];
        fourthWallDialogueInProgress = true;

        if (playerDialogueController == null)
        {
            Debug.LogError("Brak DialogueController na graczu!");
            return;
        }

        playerDialogueController.PlayDialogue();
    }

    bool IsDialogueActive()
    {
        return fourthWallDialogueInProgress || DialogueManager.DialogueInProgress;
    }
}
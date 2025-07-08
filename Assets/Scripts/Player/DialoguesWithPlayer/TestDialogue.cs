using System;
using EasyTalk.Controller;
using UnityEngine;

public class TestDialogue : MonoBehaviour
{
    [SerializeField] private string EntryPointName = "1";
    [SerializeField] private GameObject SpeechBubble;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        SetSpeechBubble(true);
        other.GetComponent<DialogueController>().PlayDialogue(EntryPointName);
        other.GetComponent<DialogueController>().onStop.AddListener(() => SetSpeechBubble(false));
    }

    private void SetSpeechBubble(bool shouldBeActivated)
    {
        SpeechBubble.SetActive(shouldBeActivated);
    }
}

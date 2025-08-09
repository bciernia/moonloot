using System;
using EasyTalk.Controller;
using UnityEngine;

public class TestDialogueWithPatron : MonoBehaviour
{
    [SerializeField] private string EntryPointName = "1";
    [SerializeField] private GameObject SpeechBubble;
    [SerializeField] private GameObject PlayerUI;

    private DialogueController _dialogueController;
    
    private void Awake()
    {
        _dialogueController = GetComponent<DialogueController>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.M))
        {
            _dialogueController.PlayDialogue(EntryPointName);
            PlayerUI.SetActive(false);
            _dialogueController.onStop.AddListener(() =>
            {
                PlayerUI.SetActive(true);
                SpeechBubble.SetActive(false);
            });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
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

    private List<string> _dialogues = new();

    private int index = 0;

    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);

        LoadDialogues();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Base")
            return;

        if (_dialogues.Count == 0)
            return;

        StartCoroutine(DialogueRoutine());
    }

    private void LoadDialogues()
    {
        if (_dialogueFile == null)
        {
            Debug.LogWarning(
                $"No dialogue file assigned for {gameObject.name}");
            return;
        }

        var dialogues =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(
                _dialogueFile.text);

        if (dialogues == null)
            return;

        _dialogues = new List<string>(dialogues.Values);
    }

    private IEnumerator DialogueRoutine()
    {
        while (true)
        {
            var delay = Random.Range(
                _minDelay,
                _maxDelay);

            yield return new WaitForSeconds(delay);

            ShowRandomDialogue();

            yield return new WaitForSeconds(_displayTime);

            HideDialogue();
        }
    }

    private void ShowRandomDialogue()
    {
        if (index >= _dialogues.Count)
            return;
        
        if (_panel == null || _text == null)
            return;

        if (_dialogues.Count == 0)
            return;

        // var index = Random.Range(
        //     0,
        //     _dialogues.Count);

        _text.text = _dialogues[index];
        index++;
        
        _panel.SetActive(true);
    }

    private void HideDialogue()
    {
        if (_panel == null)
            return;

        _panel.SetActive(false);
    }
}
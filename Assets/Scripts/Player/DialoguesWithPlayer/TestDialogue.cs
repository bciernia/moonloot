using System;
using EasyTalk.Controller;
using UnityEngine;

public class TestDialogue : MonoBehaviour
{
    [SerializeField] private string EntryPointName = "1";
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<DialogueController>().PlayDialogue(EntryPointName);
    }
}

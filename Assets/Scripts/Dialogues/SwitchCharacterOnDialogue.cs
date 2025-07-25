using EasyTalk.Controller;
using UnityEngine;

public class SwitchCharacterOnDialogue : MonoBehaviour
{
    [SerializeField] private GameObject characterToSwitch;

    public void SwitchCharacter()
    {
        var parent = transform.parent;
        gameObject.GetComponent<DialogueController>().ExitDialogue();
        Instantiate(characterToSwitch, transform.position, transform.rotation, parent);
        Destroy(gameObject);
    }
    
    
}

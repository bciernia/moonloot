using EasyTalk.Controller;
using UnityEngine;

public class SwitchCharacterOnDialogue : MonoBehaviour
{
    [SerializeField] private GameObject characterToSwitch;
    [SerializeField] private DropItem[] itemsForCharacter;
    
    public void SwitchCharacter()
    {
        var parent = transform.parent;
        gameObject.GetComponent<DialogueController>().ExitDialogue();
        var newCharacter = Instantiate(characterToSwitch, transform.position, transform.rotation, parent);
        newCharacter.GetComponent<EnemyLoot>().AddDropItemsToListFromParent(itemsForCharacter);
        Destroy(gameObject);
    }
        
}

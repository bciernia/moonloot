using System.Collections.Generic;
using UnityEngine;

public class DialogueHelper : MonoBehaviour
{
    [SerializeField] private List<GameObject> ObjectsToShowInDialogue;
    [SerializeField] private List<GameObject> EnemiesToActivate;

    public void ShowObjectInDialogue(int objectId = 0)
    {
        ObjectsToShowInDialogue[objectId].gameObject.SetActive(true);
    }
    
    public void HideObjectInDialogue(int objectId = 0)
    {
        ObjectsToShowInDialogue[objectId].gameObject.SetActive(false);
    }

    public void ActivateEnemies()
    {
        foreach (var enemy in EnemiesToActivate)
        {
            enemy.GetComponent<EnemyBrain>().enabled = true;
            enemy.tag = "Enemy";
        }
    }

    public void DeactivateEnemies()
    {
        foreach (var enemy in EnemiesToActivate)
        {
            enemy.GetComponent<EnemyBrain>().enabled = false;
            enemy.tag = "Untagged";
        }
    }
}

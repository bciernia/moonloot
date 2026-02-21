using UnityEngine;

public class ShowObjectWhenHaveDialogueFlag : MonoBehaviour
{
    [SerializeField] private string _dialogueFlag;
    
    void Start()
    {
        if (DialogueFlagsManager.Instance.IsFlagInHashSet(_dialogueFlag))
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }        
    }
}

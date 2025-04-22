using UnityEngine;

public class QuestListUI : MonoBehaviour
{
    [SerializeField] private QuestItemUI _questItem;
    private QuestList _questList;    
    
    void Start()
    {
        _questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        _questList.onUpdate += Redraw;
        Redraw();
    }

    private void Redraw()
    {
        transform.DetachChildren();
        foreach (var status in _questList.GetStatuses())
        {
            var uiInstance = Instantiate(_questItem, transform);
            uiInstance.Setup(status);
        }
    }
}

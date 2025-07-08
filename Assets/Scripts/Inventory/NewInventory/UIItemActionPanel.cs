using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIItemActionPanel : MonoBehaviour
{
    [SerializeField] private GameObject btnPrefab;

    public void AddButton(string name, Action onClickAction)
    {
        var btn = Instantiate(btnPrefab, transform);
        btn.GetComponent<Button>().onClick.AddListener(() => onClickAction());
        btn.GetComponentInChildren<TMPro.TMP_Text>().text = name;
    }

    public void Toggle(bool val)
    {
        if (val) RemoveOldButtons();
        
        gameObject.SetActive(val);
    }

    private void RemoveOldButtons()
    {
        foreach (Transform transformChildObjects in transform)
        {
            Destroy(transformChildObjects.gameObject);
        }
    }
}

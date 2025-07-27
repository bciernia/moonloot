using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillDescription : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private GameObject btnsPanel;
    
    private void Awake()
    {
        ResetDescription();
    }

    public void ResetDescription()
    {
        itemImage.gameObject.SetActive(false);
        title.text = "";
        description.text = "";
        btnsPanel.gameObject.SetActive(false);
    }

    public void SetDescription(Sprite sprite, string itemName, string itemDescription)
    {
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = sprite;
        title.text = itemName;
        description.text = itemDescription;
        btnsPanel.gameObject.SetActive(true);
    }
}

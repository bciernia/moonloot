using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceElementUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    public void Setup(Sprite sprite, string text)
    {
        icon.sprite = sprite;
        amountText.text = text;
    }
}

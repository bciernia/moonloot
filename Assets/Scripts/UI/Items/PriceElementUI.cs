using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceElementUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _text;

    [Header("Colors")]
    [SerializeField] private Color _enoughColor = Color.green;
    [SerializeField] private Color _notEnoughColor = Color.red;

    public void Setup(Sprite icon, string itemName, int requiredAmount, int ownedAmount)
    {
        _icon.sprite = icon;

        _text.text = $"{itemName} x{requiredAmount} ({ownedAmount})";

        _text.color = ownedAmount >= requiredAmount
            ? _enoughColor
            : _notEnoughColor;
    }
}

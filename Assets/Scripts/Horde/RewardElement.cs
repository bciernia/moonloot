using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardElement : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _amount;

    public void Setup(Sprite sprite, int amount)
    {
        _image.sprite = sprite;
        _amount.text = $"x {amount.ToString()}";
    }
}

[System.Serializable]
public class NightReward
{
    public ItemSO Item;
    public int Amount;
}
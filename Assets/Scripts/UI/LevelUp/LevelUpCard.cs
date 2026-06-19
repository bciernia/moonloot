using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private TMP_Text _description;

    [SerializeField]
    private Image _image;
    private LevelUpUpgradeSO _upgrade;
    
    private Vector3 _targetScale = Vector3.one;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private bool _hoverEnabled = true;
    
    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            10f * Time.unscaledDeltaTime);
    }
    
    public void Setup(LevelUpUpgradeSO upgrade)
    {
        _upgrade = upgrade;
        _title.text = upgrade.UpgradeName;
        _description.text = upgrade.Description;
        _image.sprite = upgrade.Icon;
    }

    public void OnClick()
    {
        LevelUpManager.Instance.SelectUpgrade(_upgrade);
    }
    
    public LevelUpUpgradeSO GetUpgrade()
    {
        return _upgrade;
    }
    
    public void SetInteractable(bool value)
    {
        _canvasGroup.interactable = value;
        _canvasGroup.blocksRaycasts = value;
    }
    
    public void DisableHover()
    {
        _hoverEnabled = false;
        _targetScale = Vector3.one;
    }
    
    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (!_hoverEnabled)
            return;

        _targetScale =
            Vector3.one * 1.08f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hoverEnabled)
            return;
        
        _targetScale = Vector3.one;
    }
}
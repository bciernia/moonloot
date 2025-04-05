using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager: Singleton<WeaponManager>
{
    [Header("Config")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _weaponManaTMP;

    public void EquipWeapon(Weapon weapon)
    {
        _weaponIcon.sprite = weapon.Icon;
        _weaponIcon.gameObject.SetActive(true);

        if (weapon.RequiredMana != 0)
        {
            _weaponManaTMP.text = weapon.RequiredMana.ToString(CultureInfo.InvariantCulture);
            _weaponManaTMP.gameObject.SetActive(true);
        }
        else
        {
            _weaponManaTMP.gameObject.SetActive(false);
        }

        GameManager.Instance.Player.PlayerAttack.EquipWeapon(weapon);
    }
}

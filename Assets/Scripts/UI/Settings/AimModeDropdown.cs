using TMPro;
using UnityEngine;

public class AimModeDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown.SetValueWithoutNotify(
            (int)SettingsManager.Instance.AimMode);

        dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(int value)
    {
        SettingsManager.Instance.SetAimMode(value);
    }
}
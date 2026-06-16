using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Text _volume;
    [SerializeField] private VolumeType _volumeType;

    private void Start()
    {
        _slider.value =
            PlayerPrefs.GetFloat(
                "MusicVolume",
                1f);

        OnValueChanged(_slider.value);

        UpdateVolumeText(_slider.value);

        _slider.onValueChanged.AddListener(
            OnValueChanged);
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(
            OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        switch (_volumeType)
        {
            case VolumeType.Music:
                SoundManager.Instance.SetMusicVolume(value);
                break;

            case VolumeType.Sfx:
                SoundManager.Instance.SetSfxVolume(value);
                break;
        }

        PlayerPrefs.SetFloat(
            GetPlayerPrefsKey(),
            value);

        PlayerPrefs.Save();

        UpdateVolumeText(value);
    }

    private string GetPlayerPrefsKey()
    {
        return _volumeType + "Volume";
    }

    private void UpdateVolumeText(float value)
    {
        _volume.text = $"{value * 100:0}%";
    }
}
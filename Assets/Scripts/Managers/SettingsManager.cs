using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public bool UseGamepad { get; private set; }
    
    private const string AimModeKey = "AimMode";

    public AimMode AimMode { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        AimMode = (AimMode)PlayerPrefs.GetInt(
            AimModeKey,
            (int)AimMode.Keyboard);
    }

    public void SetAimMode(int value)
    {
        AimMode = (AimMode)value;

        PlayerPrefs.SetInt(
            AimModeKey,
            value);

        PlayerPrefs.Save();
    }

    public void SetGamepadState(bool isGamepad)
    {
        UseGamepad = isGamepad;
    }
}
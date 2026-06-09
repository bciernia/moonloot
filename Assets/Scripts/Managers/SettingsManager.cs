public class SettingsManager : Singleton<SettingsManager>
{
    public AimMode AimMode { get; private set; } = AimMode.Keyboard;

    public bool UseGamepad { get; private set; }
    
    public void SetAimMode(int dropdownValue)
    {
        AimMode = (AimMode)dropdownValue;

        ES3.Save("AimMode", dropdownValue);
    }

    private void Start()
    {
        if (ES3.KeyExists("AimMode"))
        {
            AimMode = (AimMode)ES3.Load<int>("AimMode");
        }
    }

    public void SetGamepadState(bool isGamepad)
    {
        UseGamepad = isGamepad;
    }
}
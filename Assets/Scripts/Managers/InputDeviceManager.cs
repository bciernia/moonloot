using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceManager : MonoBehaviour
{
    private void Update()
    {
        if (Gamepad.current != null)
        {
            if (Gamepad.current.wasUpdatedThisFrame)
            {
                SettingsManager.Instance.SetGamepadState(true);
            }
        }

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SettingsManager.Instance.SetGamepadState(false);
        }

        if (Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            SettingsManager.Instance.SetGamepadState(false);
        }
    }
}
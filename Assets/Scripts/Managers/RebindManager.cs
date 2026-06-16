using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;

    public void SaveBindings()
    {
        var rebinds =
            actions.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString(
            "rebinds",
            rebinds);
        
        PlayerPrefs.Save();
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetAllBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActionAsset;

    public void ResetBindings()
    {
        Debug.Log("RESET BINDINGS CALLED");

        
        foreach (var map in _inputActionAsset.actionMaps)
        {
            map.RemoveAllBindingOverrides();            
        }
        PlayerPrefs.DeleteKey("rebinds");
    }
}
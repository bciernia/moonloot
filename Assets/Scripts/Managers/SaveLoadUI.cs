using UnityEngine;

public class SaveLoadUI : MonoBehaviour
{
    [field: SerializeField]
    public GameObject SlotTemplate { get; private set; }

    [field: SerializeField]
    public GameObject CreateDialog { get; private set; }

    [field: SerializeField]
    public GameObject ErrorDialog { get; private set; }
}

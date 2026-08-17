using UnityEngine;

[CreateAssetMenu(fileName = "Mutation", menuName = "Moonloot/Horde/Mutation")]
public class MutationSO : ScriptableObject
{
    [Header("Info")]
    public HordeMutation Type;
    public string DisplayName;
    
    [TextArea]
    public string Description;

    public Sprite Icon;

    [Header("Values")]
    public float Value;
}
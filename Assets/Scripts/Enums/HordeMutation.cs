using UnityEngine;

public enum HordeMutation
{
    None,
    StrongEnemies, 
    FastEnemies,   
    BrutalEnemies  
}

/*TODO Do dorobienia:
 *
 * Wybuchajacy przeciwnicy
 * Wampiryczni (leczący się)
 */
 
 [System.Serializable]
 public class MutationData
 {
     public HordeMutation Mutation;
     public string DisplayName;
     [TextArea]
     public string Description;
     public Sprite Icon;
 }
using System;
using UnityEngine;

public class NpcSaveLoad : MonoBehaviour, ISaveable
{
    [SerializeField] public string npcId;
            
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return;

        if (string.IsNullOrEmpty(npcId))
        {
            npcId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
    
    public void Save()
    {
        ES3.Save($"npc_{npcId}", transform.position);
    }

    public void Load()
    {
        if (!ES3.KeyExists($"npc_{npcId}"))
            return;
        
        transform.position = ES3.Load<Vector3>($"npc_{npcId}");
    }
}

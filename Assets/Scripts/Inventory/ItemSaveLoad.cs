using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSaveLoad : MonoBehaviour, ISaveable
{
    [SerializeField] private string itemId;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return;

        if (string.IsNullOrEmpty(itemId))
        {
            itemId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    private void Awake()
    {
        if (string.IsNullOrEmpty(itemId))
            itemId = Guid.NewGuid().ToString();
    }
    
    public void Save()
    {
        List<string> collected;

        if (ES3.KeyExists("worldItems"))
            collected = ES3.Load<List<string>>("worldItems");
        else
            collected = new List<string>();

        if (!collected.Contains(itemId))
            collected.Add(itemId);

        ES3.Save("worldItems", collected);    
    }

    public void Load()
    {
        if (!ES3.KeyExists("worldItems"))
            return;

        var collected = ES3.Load<List<string>>("worldItems");

        if (!collected.Contains(itemId))
            Destroy(gameObject);
    }
}
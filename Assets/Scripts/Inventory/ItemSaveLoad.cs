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
        var settings = SaveLoadManager.Instance.GetSettings();

        if (ES3.KeyExists("worldItems", settings))
            collected = ES3.Load<List<string>>("worldItems", settings);
        else
            collected = new List<string>();

        if (!collected.Contains(itemId))
            collected.Add(itemId);

        ES3.Save("worldItems", collected, settings);    
    }

    public void Load()
    {
        var settings = SaveLoadManager.Instance.GetSettings();
        if (!ES3.KeyExists("worldItems", settings))
            return;

        var collected = ES3.Load<List<string>>("worldItems", settings);

        if (!collected.Contains(itemId))
            Destroy(gameObject);
    }
}

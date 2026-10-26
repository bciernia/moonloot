using UnityEditor;
using UnityEngine;

public static class FixItemInteractionAssignments
{
    [MenuItem("Tools/Items/Fix CookedMeat and Salmon Inventory Items")]
    private static void Fix()
    {
        FixPrefab(
            "Assets/Prefabs/Items/Edible/Food/Dishes/CookedMeat.prefab",
            "Assets/Data/Edible Items/Food/Dishes/CookedMeat.asset");
        FixPrefab(
            "Assets/Prefabs/Items/Edible/Food/Fishes/Salmon.prefab",
            "Assets/Data/Edible Items/Food/Salmon/Salmon.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CookedMeat and Salmon ItemInteraction InventoryItem references were fixed.");
    }

    private static void FixPrefab(string prefabPath, string itemPath)
    {
        var item = AssetDatabase.LoadAssetAtPath<ItemSO>(itemPath);
        if (item == null) throw new System.Exception("Could not load ItemSO: " + itemPath);
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var interaction = root.GetComponent<ItemInteraction>();
            if (interaction == null) interaction = root.GetComponentInChildren<ItemInteraction>(true);
            if (interaction == null) throw new System.Exception("No ItemInteraction found: " + prefabPath);
            var serialized = new SerializedObject(interaction);
            var field = serialized.FindProperty("_inventoryItem");
            if (field == null) throw new System.Exception("InventoryItem field not found: " + prefabPath);
            field.objectReferenceValue = item;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
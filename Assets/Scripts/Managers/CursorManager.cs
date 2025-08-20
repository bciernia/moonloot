using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private CursorMapping[] CursorMappings = null;

    enum CursorType
    {
        None,
        Movement,
    }
    
    [Serializable]
    struct CursorMapping
    {
        public CursorType type;
        public Texture2D texture;
        public Vector2 hotspot;
    }
    
    void Start()
    {
        SetCursor(CursorType.None);
    }

    private void SetCursor(CursorType type)
    {
        var mapping = GetCursorMapping(type);
        Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
    }

    private CursorMapping GetCursorMapping(CursorType type)
    {
        foreach (var mapping in CursorMappings)
        {
            if (mapping.type == type)
            {
                return mapping;
            }
        }

        return CursorMappings[0];
    }
}

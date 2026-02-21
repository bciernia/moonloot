using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Sounds/FloorTypes", fileName = "FloorType_")]
public class TileSoundSO : ScriptableObject
{
    public TileBase[] Tiles;
    public AudioClip[] Clips;
    public FloorType FloorType;
}
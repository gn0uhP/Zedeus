using System;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class LevelPrefabEntry
{
    public string id;
    public GameObject prefab;
}

[Serializable]
public class LevelTileEntry
{
    public string id;
    public TileBase tile;
}
public class LevelManager : MonoBehaviour
{
    [Header("LevelReferences")]
    public Transform levelObjects;
    public Tilemap ground;
    public Vector3 playerSpawn;

    [Header("LevelEditor")]
    public string newLevelName = "Level_01";
    public int selectedLevelIndex;

    [Header("Registry")]
    public LevelPrefabEntry[] prefabs;
    public LevelTileEntry[] tiles;

    public GameObject getPrefabById(string id)
    {
        foreach(var entry in prefabs)
        {
            if (entry.id == id)
            {
                return entry.prefab;
            }
        }
        return null;
    }


    public string getIdbyTile(TileBase tile)
    {
        foreach(var entry in tiles)
        {
            if(entry.tile == tile)
            {
                return entry.id;
            }
        }
        return null;
    }

    public TileBase getTileBaseById(string id)
    {
        foreach(var entry in tiles)
        {
            if(entry.id == id)
            {
                return entry.tile;
            }
        }
        return null;
    }

}

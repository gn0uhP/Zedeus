using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;

[Serializable]
public class LevelTileData
{
    public string id;
    public Vector3Int position;
}

[Serializable]
public class LevelObjectData
{
    public string id;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public string parentGroup;

    public float jointRadius;
    public float dashForce;
}

[Serializable]
public class LevelData 
{
    public string levelName;
    public Vector3 playerSpawn;

    public List<LevelTileData> tiles = new();
    public List<LevelObjectData> objects = new()    ;
}




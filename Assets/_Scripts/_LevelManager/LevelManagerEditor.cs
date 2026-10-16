using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEditor.TerrainTools;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public const string prefabFolderPath = "Assets/_Prefabs/_LevelObject";
    public const string tileFolderPath = "Assets/_Tiles";
    public const string levelFolderPath = "Assets/_Levels";

    private Player player = FindFirstObjectByType<Player>();
    private LevelManager manager;
    public string[] levelList = Array.Empty<string>();
    private void OnEnable()
    {
        manager = (LevelManager)target;
        RefreshLevelList();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(15);

        if(GUILayout.Button("RefreshRegistry", GUILayout.Height(30)))
        {
            RefreshRegister();
        }

        if(GUILayout.Button("RefreshLevelList", GUILayout.Height(30)))
        {
            RefreshLevelList();
        }

        if(GUILayout.Button("SaveNewLevel", GUILayout.Height(30)))
        {
            SaveLevel(false);
        }

        if(GUILayout.Button("OverWriteSaveFile", GUILayout.Height(30)))
        {
            SaveLevel(true);
        }

        string[] levelName = levelList.Select(Path.GetFileNameWithoutExtension).ToArray();

        int currentIndex = Mathf.Clamp(manager.selectedLevelIndex, 0, levelName.Length - 1);

        int index = EditorGUILayout.Popup("SelectedLevel", currentIndex, levelName);

        if(index != manager.selectedLevelIndex)
        {
            manager.selectedLevelIndex = index;
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }

        if(GUILayout.Button("LoadSelectedLevel", GUILayout.Height(30)))
        {
            LoadLevel();
        }
    }

    private void RefreshLevelList()
    {
        levelList = Directory.GetFiles(levelFolderPath, "*.json").OrderBy(x => x, StringComparer.Ordinal).ToArray();
    }

    private void RefreshRegister()
    {
        List<LevelPrefabEntry> prefabList = new();
        string[] prefabGuid = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });
        
        foreach(string guid in prefabGuid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            string id = prefab.GetComponent<LevelObject>().id;

            prefabList.Add(new LevelPrefabEntry { id = id, prefab = prefab});
        }

        List<LevelTileEntry> tileList = new();
        string[] tileGuid = AssetDatabase.FindAssets("t:Tile", new[] { tileFolderPath });

        foreach(string guid in tileGuid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);

            string id = tile.name;

            tileList.Add(new LevelTileEntry { id = id, tile = tile });
        }

        prefabList = prefabList.OrderBy(x => x.id).ToList();
        tileList = tileList.OrderBy(x => x.id).ToList();

        manager.prefabs = prefabList.ToArray();
        manager.tiles = tileList.ToArray();

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

    }

    private void SaveLevel(bool overwrite)
    {
        string levelName;

        if (overwrite)
        {
            int index = Mathf.Clamp(manager.selectedLevelIndex, 0, levelList.Length - 1);
            levelName = Path.GetFileNameWithoutExtension(levelList[index]);
        }

        else
        {
            levelName = GetLevelName();
        }

        LevelData data = new LevelData();

        data.levelName = levelName;
        data.playerSpawn = player.transform.position;

        foreach(Vector3Int cell in manager.ground.cellBounds.allPositionsWithin)
        {
            TileBase tile = manager.ground.GetTile(cell);
            if (tile == null) continue;

            string id = manager.getIdbyTile(tile);

            data.tiles.Add(new LevelTileData { id = id, position = cell });
        }

        LevelObject[] objects = manager.levelObjects.GetComponentsInChildren<LevelObject>(true);

        foreach(LevelObject obj in objects)
        {
            Transform parent = obj.transform.parent;
            LevelObjectData lvData = new LevelObjectData
            {
                id = obj.id,
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,

                parentGroup = parent.name
            };

            Anchor anchor = obj.GetComponent<Anchor>();
            if(anchor != null)
            {
                lvData.jointRadius = anchor.jointRadius;
            }

            LightBall lightBall = obj.GetComponent<LightBall>();
            if(lightBall != null)
            {
                lvData.jointRadius = lightBall.jointRadius;
                lvData.dashForce = lightBall.dashForce;
            }
            data.objects.Add(lvData);
        }

        string path = Path.Combine(levelFolderPath, levelName + ".json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        RefreshLevelList();
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }

    private void LoadLevel()
    {
        RefreshLevelList();

        int index = Mathf.Clamp(manager.selectedLevelIndex, 0, levelList.Length - 1);

        string path = levelList[index];
        string json = File.ReadAllText(path);
        LevelData data;
        try
        {
            data = JsonUtility.FromJson<LevelData>(json);
        }
        catch(Exception e)
        {
            Debug.Log(e);
            return;
        }

        LevelObject[] oldObject = manager.levelObjects.GetComponentsInChildren<LevelObject>(true);
        foreach(LevelObject obj in oldObject)
        {
            DestroyImmediate(obj.gameObject);
        }

        manager.ground.ClearAllTiles();

        foreach(LevelObjectData obj in data.objects)
        {
            GameObject prefab = manager.getPrefabById(obj.id);

            Transform parent = manager.levelObjects.Find(obj.parentGroup);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);

            instance.transform.position = obj.position;
            instance.transform.rotation = obj.rotation;
            instance.transform.localScale = obj.scale;

            Anchor anchor = instance.GetComponent<Anchor>();
            if(anchor != null)
            {
                anchor.jointRadius = obj.jointRadius;
            }

            LightBall lightBall = instance.GetComponent<LightBall>();
            if(lightBall != null)
            {
                lightBall.jointRadius = obj.jointRadius;
                lightBall.dashForce = obj.dashForce;
            }
        }

        foreach(LevelTileData tile in data.tiles)
        {
            TileBase tileBase = manager.getTileBaseById(tile.id);

            manager.ground.SetTile(tile.position, tileBase);
        }

        player.transform.position = data.playerSpawn;

        manager.newLevelName = data.levelName;

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }

    private string GetLevelName()
    {
        int index = 1;
        while(File.Exists(Path.Combine(levelFolderPath, $"Level_{index:D2}.json")))
        {
            index++;
        }
        return $"Level_{index:D2}";
    }
}

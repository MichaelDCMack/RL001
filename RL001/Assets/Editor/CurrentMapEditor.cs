using System;
using System.IO;
using Code;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurrentMap))]
public class CurrentMapEditor : Editor
{
    enum PaintMode
    {
        Off,
        On
    }

    int newSizeX;
    int newSizeY;
    int selectedBrush;

    Texture2D[] croppedTextures;
    PaintMode paintMode;

    bool IsDelegateRegistered()
    {
        if(SceneView.onSceneGUIDelegate != null)
        {
            foreach(Delegate d in SceneView.onSceneGUIDelegate.GetInvocationList())
            {
                if(d == (SceneView.OnSceneFunc)OnScene)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void OnEnable()
    {
        CurrentMap cm = (CurrentMap)target;
        MapRenderer mr = cm.GetComponent<MapRenderer>();

        if(cm.map != null)
        {
            newSizeX = cm.map.sizeX;
            newSizeY = cm.map.sizeY;
        }

        if(!IsDelegateRegistered())
        {
            SceneView.onSceneGUIDelegate += OnScene;
        }

        int numTextures = mr.spriteMapper.tileSpriteMapData.Length;
        croppedTextures = new Texture2D[numTextures];
        for(int i = 0; i < numTextures; ++i)
        {
            Sprite sprite = mr.spriteMapper.tileSpriteMapData[i].sprite;
            Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x, 
                                                      (int)sprite.rect.y, 
                                                      (int)sprite.rect.width, 
                                                      (int)sprite.rect.height);

            croppedTextures[i] = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            croppedTextures[i].SetPixels(pixels);
            croppedTextures[i].Apply();
        }
    }

    void OnDisable()
    {
        while(IsDelegateRegistered())
        {
            SceneView.onSceneGUIDelegate -= OnScene;
        }
    }

    void OnScene(SceneView sceneView)
    {
        CurrentMap cm = (CurrentMap)target;
        MapRenderer mr = cm.GetComponent<MapRenderer>();

        if(Event.current.type == EventType.MouseDown)
        {
            paintMode = PaintMode.On;
        }

        if(Event.current.type == EventType.MouseUp)
        {
            paintMode = PaintMode.Off;
        }

        if(Event.current.isMouse)
        {
            if(paintMode == PaintMode.On)
            {
                Vector2 mousePos = Event.current.mousePosition;

                mousePos.y = Camera.current.pixelHeight - mousePos.y;

                Vector3 position = Camera.current.ScreenToWorldPoint(mousePos);
                Vector2 mapPos = mr.WorldPosToMapPos(position);

                if(mapPos.x >= 0 && mapPos.x < cm.map.sizeX && mapPos.y >= 0 && mapPos.y < cm.map.sizeY)
                {
                    cm.map[(int)mapPos.x, (int)mapPos.y].TileType = 
                        mr.spriteMapper.tileSpriteMapData[selectedBrush].tileTypes[0];
                }

                EditorUtility.SetDirty(target);
            }

            Event.current.Use();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CurrentMap cm = (CurrentMap)target;

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Resize Map"))
        {
            Map newMap = new Map(newSizeX, newSizeY);

            newMap.StampMap(cm.map, Vector2.zero);
            newMap.BuildPatchRoom();

            cm.map = newMap;
            cm.generator.ResetUsedCounts();

            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.LabelField("x", GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));
        newSizeX = EditorGUILayout.IntField(newSizeX);

        EditorGUILayout.LabelField("y", GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));
        newSizeY = EditorGUILayout.IntField(newSizeY);

        EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Build Random Join"))
        {
            cm.generator.BuildRandomJoinTile(cm.map);

            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Build Next Random Sub Map"))
        {
            cm.generator.BuildNextRandomSubMap(cm.map);

            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Post Process Map"))
        {
            cm.generator.PostProcessMap(cm.map);

            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Clear Map"))
        {
            cm.generator.ClearMap(cm.map);

            EditorUtility.SetDirty(target);
        }
            
        if(GUILayout.Button("Save Map"))
        {
            EditorApplication.delayCall += SaveCurrentMapToFile;
        }

        selectedBrush = GUILayout.SelectionGrid(selectedBrush, croppedTextures, 5);
    }

    public void SaveCurrentMapToFile()
    {
        CurrentMap cm = (CurrentMap)target;

        string path = EditorUtility.SaveFilePanelInProject("Save Map", "New Map.txt", "txt", "");
        string s = cm.map.WriteGlyphs();

        File.WriteAllText(path, s);
        AssetDatabase.Refresh();
    }
}
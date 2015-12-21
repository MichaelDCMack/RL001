using Code;
using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(CurrentMap))]
public class CurrentMapEditor : Editor
{
    int newSizeX;
    int newSizeY;

    void OnEnable()
    {
        CurrentMap cm = (CurrentMap)target;

        newSizeX = cm.map.sizeX;
        newSizeY = cm.map.sizeY;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CurrentMap cm = (CurrentMap)target;

        Rect rect = EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Resize Map"))
        {
            Map newMap = new Map(newSizeX, newSizeY);

            newMap.StampMap(cm.map, new Point(0, 0));
            newMap.FindLinkPoints();
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
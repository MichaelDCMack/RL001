using Code;
using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(CurrentMap))]
public class CurrentMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CurrentMap cm = (CurrentMap)target;

        if(GUILayout.Button("Resize Map"))
        {
            Map newMap = new Map(cm.sizeX, cm.sizeY);
            newMap.StampMap(cm.map, new Point(0, 0));
            newMap.FindLinkPoints();
            cm.map = newMap;
            cm.generator.ResetUsedCounts();
            EditorUtility.SetDirty(target);
        }

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
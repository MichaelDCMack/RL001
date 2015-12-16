using UnityEngine;
using Code;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator mg = (MapGenerator)target;

        if(GUILayout.Button("Build Random Join"))
        {
            mg.BuildRandomJoinTile();
            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Build Next Random Sub Map"))
        {
            mg.BuildNextRandomSubMap();
            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Post Process Map"))
        {
            mg.PostProcessMap();
            EditorUtility.SetDirty(target);
        }

        if(GUILayout.Button("Clear Map"))
        {
            mg.ClearMap();

            foreach(MapGeneratorData mgd in mg.mapDataArray)
            {
                mgd.TimesUsed = 0;
            }
            EditorUtility.SetDirty(target);
        }
    }
}
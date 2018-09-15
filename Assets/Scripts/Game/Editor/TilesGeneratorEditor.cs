
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapManager))]
public class TilesGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Generate Tiles"))
			(target as MapManager).LoadMap("Map1");
	}
}

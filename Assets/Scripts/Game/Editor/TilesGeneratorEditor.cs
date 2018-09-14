
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilesGenerator))]
public class TilesGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Generate Tiles"))
			(target as TilesGenerator).GenerateTiles();
	}
}

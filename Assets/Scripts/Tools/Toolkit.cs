
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;


static public class Toolkit
{
	static public float Sqrd(this float fMe)
	{
		return fMe * fMe;
	}


#if UNITY_EDITOR
	[MenuItem("Tools/Reserialize all files")]
	static private void ReserializeAllFiles()
	{
		string[] pAllDataObjectsGUIDs = AssetDatabase.FindAssets("", new string[] { "Assets" });

		List<string> pDataObjectsPaths = new List<string>(pAllDataObjectsGUIDs.Length);
		for (int i = 0; i < pAllDataObjectsGUIDs.Length; ++i)
		{
			string sPath = AssetDatabase.GUIDToAssetPath(pAllDataObjectsGUIDs[i]);
			pDataObjectsPaths.Add(sPath);
		}

		AssetDatabase.ForceReserializeAssets(pDataObjectsPaths, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
	}
#endif
}

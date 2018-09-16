
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;


static public class Toolkit
{
	static public bool InitSingleton<T>(T pCurrentInstance, ref T r_pInstanceObject) where T : MonoBehaviour
	{
		if (r_pInstanceObject != null)
		{
			if (pCurrentInstance != r_pInstanceObject)
			{
#if UNITY_EDITOR
				EditorUtility.DisplayDialog("Singleton duplicate found", "A second instance of " + pCurrentInstance.GetType() + " on the object \"" + pCurrentInstance.name + "\" has been found and has been destroyed. Please remove one of them from the scene", "Will do");
#endif
				GameObject.Destroy(pCurrentInstance);
			}

			return false;
		}

		r_pInstanceObject = pCurrentInstance;
		return true;
	}


	static public float Sqrd(this float fMe)
	{
		return fMe * fMe;
	}

	static public Vector3 Rotate90AroundY(this Vector3 tVec)
	{
		return new Vector3(-tVec.z, tVec.y, tVec.x);
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

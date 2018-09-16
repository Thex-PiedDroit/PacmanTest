
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
				DisplayImportantErrorMessage("Singleton duplicate found", "A second instance of " + pCurrentInstance.GetType() + " on the object \"" + pCurrentInstance.name + "\" has been found and has been destroyed. Please remove one of them from the scene");
				GameObject.Destroy(pCurrentInstance);
			}

			return false;
		}

		r_pInstanceObject = pCurrentInstance;
		return true;
	}

	static public void DisplayImportantErrorMessage(string sMessageHeader, string sMessage)
	{
#if UNITY_EDITOR
		EditorUtility.DisplayDialog(sMessageHeader, sMessage, "Oki doki");
#endif
		Debug.LogError(sMessage);
	}

	static public Vector3 FlattenDirectionOnOneAxis(Vector3 tDirection)
	{
		float fX = 0.0f;
		float fZ = 0.0f;

		if (tDirection.x.Sqrd() >= tDirection.z.Sqrd())
			fX = tDirection.x > 0.0f ? 1.0f : -1.0f;
		else
			fZ = tDirection.z > 0.0f ? 1.0f : -1.0f;

		return new Vector3(fX, 0.0f, fZ);
	}

	static public Vector3 QueryMoveDirectionInput()
	{
		float fHorizontal = Input.GetAxis("Horizontal");
		float fVertical = Input.GetAxis("Vertical");

		Vector3 tInputVector = new Vector3(fHorizontal, 0.0f, fVertical);

		return tInputVector.sqrMagnitude > 0.0025f ? tInputVector.normalized : Vector3.zero;
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

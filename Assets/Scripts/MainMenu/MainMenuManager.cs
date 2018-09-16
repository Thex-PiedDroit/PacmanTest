
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class MainMenuManager : MonoBehaviour
{
	#region Variables (public)

	public string m_sMainSceneName = "MainScene";

	#endregion

#region Variables (private)



	#endregion


	public void LaunchGame()
	{
#if UNITY_EDITOR
		if (SceneUtility.GetBuildIndexByScenePath("Scenes/" + m_sMainSceneName) == -1)
		{
			Toolkit.DisplayImportantErrorMessage("Invalid main scene name", "The scene name specified in the " + GetType() + " was not found in the Scenes folder.\n\nMake sure the " + GetType() + " has the right name and the scene is in the Scenes folder please.");
			return;
		}
#endif
		SceneManager.LoadScene(m_sMainSceneName);
	}

	public void QuitGame()
	{
		Application.Quit();		// TODO: Might want to add a "Are you sure?" popup later on
	}
}

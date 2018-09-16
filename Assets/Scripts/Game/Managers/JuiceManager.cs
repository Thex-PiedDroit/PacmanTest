
using UnityEngine;
using System.Collections;


public class JuiceManager : MonoBehaviour
{
#region Variables (public)

	static public JuiceManager Instance = null;

	#endregion

#region Variables (private)

	private bool m_bFreezingFrame = false;

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;
	}

	public void FreezeFrame(float fDurationInSeconds)
	{
		if (m_bFreezingFrame)
		{
			Debug.LogError("Someone is asking a freeze frame even though there's one still going. Something is not right");
			return;
		}

		StartCoroutine(FreezeFrameForSeconds(fDurationInSeconds));
	}

	private IEnumerator FreezeFrameForSeconds(float fSeconds)
	{
		m_bFreezingFrame = true;

		Time.timeScale = 0.0f;

		float fStartTime = Time.unscaledTime;
		while (Time.unscaledTime - fStartTime < fSeconds)
			yield return false;

		Time.timeScale = 1.0f;

		m_bFreezingFrame = false;
	}
}


using TMPro;
using UnityEngine;
using System;
using System.Collections;


public class CountDownModule : MonoBehaviour
{
#region Variables (public)

	public Action OnCountDownFinished = null;


	public GameObject m_pCountDownBackGround = null;
	public TextMeshProUGUI m_pCountDownText = null;

	#endregion

#region Variables (private)

	private bool m_bCurrentlyCountingDown = false;

	#endregion


	public void LaunchCountDown(int iDuration)
	{
		if (m_bCurrentlyCountingDown)
		{
			Debug.LogError("A countdown has been asked for even though another one is still in progress");
			return;
		}

		StartCoroutine(DisplayCountDown(iDuration));
	}

	private IEnumerator DisplayCountDown(int iDuration)
	{
		m_bCurrentlyCountingDown = true;
		m_pCountDownBackGround.SetActive(true);

		float fStartTime = Time.time;
		float fElapsed = 0.0f;

		while ((fElapsed = Time.time - fStartTime) < iDuration)
		{
			m_pCountDownText.text = (iDuration - (int)fElapsed).ToString();
			yield return false;
		}

		m_bCurrentlyCountingDown = false;
		m_pCountDownBackGround.SetActive(false);

		OnCountDownFinished?.Invoke();
	}
}

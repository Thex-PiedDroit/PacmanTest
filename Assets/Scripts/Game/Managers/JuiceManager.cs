
using UnityEngine;
using System.Collections;


public class JuiceManager : MonoBehaviour
{
#region Variables (public)

	static public JuiceManager Instance = null;


	public Camera m_pCamera = null;

	#endregion

#region Variables (private)

	private Coroutine m_pCameraZoomCoroutine = null;
	private float m_fDefaultCameraZoom = 0.0f;

	private bool m_bFreezingFrame = false;

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;

		m_fDefaultCameraZoom = m_pCamera.orthographicSize;
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

	public void ZoomCameraForOneSecond(Vector3 tTarget, float fZoomChange, float fDuration, float fImpulseStretch)
	{
		if (m_pCameraZoomCoroutine != null)
			StopCoroutine(m_pCameraZoomCoroutine);

		m_pCameraZoomCoroutine = StartCoroutine(ZoomCameraOnTarget(tTarget, fZoomChange, fDuration, fImpulseStretch));
	}

	private IEnumerator ZoomCameraOnTarget(Vector3 tTarget, float fZoomChange, float fDuration, float fImpulseStretch)
	{
		float fStartTime = Time.time;
		float fElapsed = 0.0f;

		Vector3 tCameraPosAtStart = m_pCamera.transform.position;

		while ((fElapsed = Time.time - fStartTime) < fDuration)
		{
			float fImpulse = Impulse(fElapsed, fImpulseStretch);
			m_pCamera.orthographicSize = m_fDefaultCameraZoom - (fZoomChange * fImpulse);

			Vector3 tPosThisFrame = tCameraPosAtStart + (tTarget * fImpulse);
			tPosThisFrame.y = tCameraPosAtStart.y;
			m_pCamera.transform.position = tPosThisFrame;

			yield return false;
		}

		m_pCamera.orthographicSize = m_fDefaultCameraZoom;
		m_pCamera.transform.position = tCameraPosAtStart;

		m_pCameraZoomCoroutine = null;
	}

	private float Impulse(float fElapsed, float fStretch)
	{
		float fHeight = fStretch * fElapsed;
		return fHeight * Mathf.Exp(1.0f - fHeight);
	}
}

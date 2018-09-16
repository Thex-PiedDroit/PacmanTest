
using UnityEngine;
using System.Collections;


// The code is kind of a mess here.. sacrificed cleanness for results for the very last hours of work in here

public class JuiceManager : MonoBehaviour
{
#region Variables (public)

	static public JuiceManager Instance = null;


	public Camera m_pCamera = null;

	public Transform m_pSpriteToShowBehindTargetWhenCameraZoom = null;
	public float m_fSpriteRotationSpeedInDegreesPerSecond = 180.0f;

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

	public void ZoomCameraForOneSecond(Transform pTarget, float fZoomChange, float fDuration, float fImpulseStretch, bool bShowSpriteBehindTarget = false)
	{
		if (m_pCameraZoomCoroutine != null)
			StopCoroutine(m_pCameraZoomCoroutine);

		m_pCameraZoomCoroutine = StartCoroutine(ZoomCameraOnTarget(pTarget, fZoomChange, fDuration, fImpulseStretch, bShowSpriteBehindTarget));
	}

	private IEnumerator ZoomCameraOnTarget(Transform pTarget, float fZoomChange, float fDuration, float fImpulseStretch, bool bShowSpriteBehindTarget)
	{
		float fStartTime = Time.time;
		float fElapsed = 0.0f;

		Vector3 tCameraPosAtStart = m_pCamera.transform.position;
		Vector3 tTargetPosAtStart = pTarget.position;

		if (bShowSpriteBehindTarget)
			InitSpriteDisplay(pTarget);


		while ((fElapsed = Time.time - fStartTime) < fDuration)
		{
			float fImpulse = Impulse(fElapsed, fImpulseStretch);
			m_pCamera.orthographicSize = m_fDefaultCameraZoom - (fZoomChange * fImpulse);

			Vector3 tPosThisFrame = tCameraPosAtStart + (tTargetPosAtStart * fImpulse);
			tPosThisFrame.y = tCameraPosAtStart.y;
			m_pCamera.transform.position = tPosThisFrame;

			if (bShowSpriteBehindTarget)
				UpdateSpriteDisplay(pTarget, tTargetPosAtStart, fImpulse);

			yield return false;
		}

		m_pCamera.orthographicSize = m_fDefaultCameraZoom;
		m_pCamera.transform.position = tCameraPosAtStart;

		m_pSpriteToShowBehindTargetWhenCameraZoom.gameObject.SetActive(false);

		m_pCameraZoomCoroutine = null;
	}

	private void InitSpriteDisplay(Transform pTarget)
	{
		m_pSpriteToShowBehindTargetWhenCameraZoom.gameObject.SetActive(true);

		Vector3 tRandomSpriteRotation = m_pSpriteToShowBehindTargetWhenCameraZoom.localEulerAngles;
		tRandomSpriteRotation.z = Random.Range(0.0f, 360.0f);
		m_pSpriteToShowBehindTargetWhenCameraZoom.localEulerAngles = tRandomSpriteRotation;
	}

	private void UpdateSpriteDisplay(Transform pTarget, Vector3 tTargetPosAtStart, float fImpulse)
	{
		const float c_fHeightChange = 2.0f;     // Move ghost higher to correctly display sprite behind it
		pTarget.position += (Vector3.up * c_fHeightChange);

		m_pSpriteToShowBehindTargetWhenCameraZoom.position = pTarget.position + (Vector3.down * (c_fHeightChange * 0.5f));

		Vector3 tNewScale = Vector3.one * fImpulse;
		tNewScale.z = 1.0f;
		m_pSpriteToShowBehindTargetWhenCameraZoom.localScale = tNewScale;

		Vector3 tRandomSpriteRotation = m_pSpriteToShowBehindTargetWhenCameraZoom.localEulerAngles;
		tRandomSpriteRotation.z += (fImpulse * m_fSpriteRotationSpeedInDegreesPerSecond) * Time.unscaledDeltaTime;
		m_pSpriteToShowBehindTargetWhenCameraZoom.localEulerAngles = tRandomSpriteRotation;
	}

	private float Impulse(float fElapsed, float fStretch)
	{
		float fHeight = fStretch * fElapsed;
		return fHeight * Mathf.Exp(1.0f - fHeight);
	}
}

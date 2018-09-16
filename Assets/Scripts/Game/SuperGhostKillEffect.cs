
using UnityEngine;
using System.Collections;


// Given more time, i would have made one ScriptableObject per effect and allow designers to design sequences as they want, but unfortunately it's getting really late and sleepy

[CreateAssetMenu(fileName = "SuperGhostKillEffect", menuName = "SuperGhostKillEffect")]
public class SuperGhostKillEffect : ScriptableObject
{
#region Variables (public)

	public float m_fCameraZoomChange = 8.0f;
	public float m_fCameraZoomDuration = 1.5f;
	public float m_fCameraZoomCurveStretch = 8.0f;

	public bool m_bShowSpriteBehindGhostWhileZooming = true;

	[Space()]
	public float m_fFreezeFrameDuration = 0.1f;
	public float m_fDelayBetweenCameraZoomAndFreezeFrame = 0.2f;

	#endregion

#region Variables (private)

	private bool m_bDoingEffects = false;

	#endregion


	public void HookEffect(PlayerCharacter pPlayer)
	{
		pPlayer.OnKilledGhost += TriggerEffect;
	}

	public void DetachEffect(PlayerCharacter pPlayer)
	{
		pPlayer.OnKilledGhost -= TriggerEffect;
	}

	private void TriggerEffect(Ghost pGhost)
	{
		if (m_bDoingEffects)
			return;     // Don't handle this yet. Might add it later though

		PlayerCharacter.Instance.StartCoroutine(TriggerEffects(pGhost));
	}

	private IEnumerator TriggerEffects(Ghost pGhost)
	{
		m_bDoingEffects = true;

		JuiceManager.Instance.ZoomCameraForOneSecond(pGhost.transform, m_fCameraZoomChange, m_fCameraZoomDuration, m_fCameraZoomCurveStretch, m_bShowSpriteBehindGhostWhileZooming);

		float fStartTime = Time.time;
		while (Time.time - fStartTime < m_fDelayBetweenCameraZoomAndFreezeFrame)
			yield return false;

		JuiceManager.Instance.FreezeFrame(m_fFreezeFrameDuration);

		m_bDoingEffects = false;
	}
}

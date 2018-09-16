
using UnityEngine;
using System.Collections;


// Given more time, i would have made one ScriptableObject per effect and allow designers to design sequences as they want, but unfortunately it's getting really late and sleepy

[CreateAssetMenu(fileName = "SuperGhostKillEffect", menuName = "SuperGhostKillEffect")]
public class SuperGhostKillEffect : ScriptableObject
{
#region Variables (public)

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

		JuiceManager.Instance.FreezeFrame(m_fFreezeFrameDuration);
	}
}

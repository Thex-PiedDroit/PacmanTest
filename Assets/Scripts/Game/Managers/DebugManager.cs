
using UnityEngine;


public class DebugManager : MonoBehaviour
{
#region Variables (public)

	public GrapplingHookEffect m_pGrapplingHookEffect = null;

	#endregion

#region Variables (private)



	#endregion


	public void GiveGrapplingHookToPlayer()
	{
		if (GameManager.Instance.IsGamePlaying() && !PlayerCharacter.Instance.m_pPickupsModule.HasActiveEffect(m_pGrapplingHookEffect))
			m_pGrapplingHookEffect.GiveEffectToPlayer(PlayerCharacter.Instance.m_pPickupsModule);
	}
}

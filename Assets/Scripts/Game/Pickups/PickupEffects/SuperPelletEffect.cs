
using UnityEngine;


[CreateAssetMenu(fileName = "SuperPelletEffect", menuName = "PickupEffects/SuperPellet")]
public class SuperPelletEffect : PickupEffect
{
#region Variables (public)

	public float m_fEffectDurationInSeconds = 6.0f;

	#endregion

#region Variables (private)

	private const string c_sVariableName_fSuperPelletStartTime = "fSuperPelletStartTime";

	#endregion


	override public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.SetVariable(c_sVariableName_fSuperPelletStartTime, Time.time);

		if (!pPickupsModule.HasActiveEffect(this))
		{
			pPickupsModule.GiveActiveEffect(this);
			PlayerCharacter.Instance.CanKillGhosts = true;
			PlayerCharacter.Instance.m_pNavMeshObstacle.carving = true;		// So ghosts won't try to get through the player when trying to go away from him
			GameManager.Instance.GiveFleeBehaviourToGhosts();
		}
	}

	override public void UpdateEffect(PlayerPickupsModule pPickupsModule)
	{
		float fStartTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fSuperPelletStartTime, 0.0f));

		if (Time.time - fStartTime >= m_fEffectDurationInSeconds)
			EffectEnd(pPickupsModule);
	}

	override protected void EffectEnd(PlayerPickupsModule pPickupsModule)
	{
		GameManager.Instance.ResetGhostsToNormalBehaviour();
		pPickupsModule.RemoveActiveEffect(this);
		PlayerCharacter.Instance.CanKillGhosts = false;
		PlayerCharacter.Instance.m_pNavMeshObstacle.carving = false;
	}
}

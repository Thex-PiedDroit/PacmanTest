
using UnityEngine;


[CreateAssetMenu(fileName = "CowardGhost", menuName = "GhostsBehaviours/Coward")]
public class CowardGhostBehaviour : GhostBehaviour
{
#region Variables (public)

	public float m_fDistanceFromPlayerToStartFleeingInMeters = 8.0f;
	public float m_fDistanceFromPlayerToStartFollowingAgainInMeters = 12.0f;

	#endregion

#region Variables (private)

	private const string c_sVariableName_bCowardIsFleeing = "bCowardIsFleeing";

	#endregion


	override public void InitBehaviour(Ghost pGhost)
	{
		base.InitBehaviour(pGhost);

		pGhost.m_pProceduralVariablesModule.SetVariable(c_sVariableName_bCowardIsFleeing, false);
	}

	public override void UpdateGhostBehaviour(Ghost pGhost)
	{
		if (pGhost.IsDead())
			return;

		float fSqrdDistanceFromPlayer = (pGhost.transform.position - PlayerCharacter.Instance.transform.position).sqrMagnitude;

		bool bShouldFlee = ShouldFlee(pGhost, fSqrdDistanceFromPlayer);
		pGhost.m_pProceduralVariablesModule.SetVariable(c_sVariableName_bCowardIsFleeing, bShouldFlee);

		if (bShouldFlee)
		{
			Vector3 tPosFurtherToPlayer = MapManager.Instance.GetPosOnMapFurtherFromPosition(PlayerCharacter.Instance.transform.position);  // TODO: Find a better way to flee, this is a bit lackluster
			pGhost.m_pNavMeshAgent.SetDestination(tPosFurtherToPlayer);
		}
		else
		{
			pGhost.m_pNavMeshAgent.SetDestination(PlayerCharacter.Instance.transform.position);
		}
	}

	private bool ShouldFlee(Ghost pGhost, float fSqrdDistanceFromPlayer)
	{
		bool bIsCurrentlyFleeing = pGhost.m_pProceduralVariablesModule.GetVariableAsBool(c_sVariableName_bCowardIsFleeing);

		bool bShouldFlee = bIsCurrentlyFleeing;

		if (bIsCurrentlyFleeing)
		{
			if (fSqrdDistanceFromPlayer >= m_fDistanceFromPlayerToStartFollowingAgainInMeters.Sqrd())
				bShouldFlee = false;
		}
		else if (fSqrdDistanceFromPlayer <= m_fDistanceFromPlayerToStartFleeingInMeters.Sqrd())
		{
			bShouldFlee = true;
		}

		return bShouldFlee;
	}
}

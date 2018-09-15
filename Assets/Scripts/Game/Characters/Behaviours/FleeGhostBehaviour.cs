
using UnityEngine;


[CreateAssetMenu(fileName = "FleeGhostBehaviour", menuName = "GhostsBehaviours/Flee")]
public class FleeGhostBehaviour : GhostBehaviour
{
#region Variables (public)

	public float m_fSpeedMultiplier = 0.6f;

	#endregion

#region Variables (private)



	#endregion


	override public void InitBehaviour(Ghost pGhost)
	{
		base.InitBehaviour(pGhost);

		pGhost.m_pNavMeshAgent.speed = pGhost.m_fRegularSpeed * m_fSpeedMultiplier;
	}

	override public void UpdateGhostBehaviour(Ghost pGhost)
	{
		Vector3 tPosFurtherToPlayer = MapManager.Instance.GetPosOnMapFurtherFromPosition(PlayerCharacter.Instance.transform.position);	// TODO: Find a better way to flee, this is a bit lackluster
		pGhost.m_pNavMeshAgent.SetDestination(tPosFurtherToPlayer);
	}
}

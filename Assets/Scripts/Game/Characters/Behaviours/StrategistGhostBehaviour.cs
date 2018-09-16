
using UnityEngine;
using UnityEngine.AI;


[CreateAssetMenu(fileName = "StrategistGhost", menuName = "GhostsBehaviours/Strategist")]
public class StrategistGhostBehaviour : GhostBehaviour
{
#region Variables (public)



	#endregion

#region Variables (private)



	#endregion


	override public void InitBehaviour(Ghost pGhost)
	{
		base.InitBehaviour(pGhost);

		pGhost.m_pNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
		pGhost.m_pNavMeshAgent.avoidancePriority -= 20; // This won't do, unity's navmesh system won't make them repath if obstacles are moving. Would need a custom navigation system
	}

	override public void UpdateGhostBehaviour(Ghost pGhost)
	{
		if (pGhost.IsDead())
			return;

		const float c_fDistanceInFrontOfPlayer = 0.3f;

		PlayerCharacter pCharacter = PlayerCharacter.Instance;
		pGhost.m_pNavMeshAgent.SetDestination(pCharacter.transform.position + (pCharacter.transform.forward * c_fDistanceInFrontOfPlayer));
	}
}

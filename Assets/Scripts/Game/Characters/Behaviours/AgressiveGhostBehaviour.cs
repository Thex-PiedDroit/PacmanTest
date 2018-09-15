
using UnityEngine;


[CreateAssetMenu(fileName = "AgressiveGhost", menuName = "GhostsBehaviours/Agressive")]
public class AgressiveGhostBehaviour : GhostBehaviour
{
#region Variables (public)



	#endregion

#region Variables (private)



	#endregion


	override public void UpdateGhostDestination(Ghost pGhost)
	{
		pGhost.m_pNavMeshAgent.SetDestination(PlayerCharacter.Instance.transform.position);
	}
}

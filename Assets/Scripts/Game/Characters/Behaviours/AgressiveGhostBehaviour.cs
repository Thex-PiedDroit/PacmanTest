
using UnityEngine;


[CreateAssetMenu(fileName = "AgressiveGhost", menuName = "GhostsBehaviours/Agressive")]
public class AgressiveGhostBehaviour : GhostBehaviour
{
#region Variables (public)

	public float m_fEnrageSpeedMultiplier = 1.5f;
	public int m_iPelletsCollectedCountToEnrage = 0;

	#endregion

#region Variables (private)

	private const string c_sVariableName_iPelletsCollectedByPlayerAtBehaviourStart = "iPelletsCollectedByPlayerAtBehaviourStart";
	private const string c_sVariableName_bIsEnraged = "bIsEnraged";

	#endregion


	override public void InitBehaviour(Ghost pGhost)
	{
		base.InitBehaviour(pGhost);

		ResetGhostBehaviour(pGhost);
	}

	override public void ResetGhostBehaviour(Ghost pGhost)
	{
		pGhost.m_pProceduralVariablesModule.SetVariable(c_sVariableName_iPelletsCollectedByPlayerAtBehaviourStart, PlayerCharacter.Instance.m_pPickupsModule.GetCurrentPelletsCount());
		pGhost.m_pProceduralVariablesModule.SetVariable(c_sVariableName_bIsEnraged, false);
	}

	override public void UpdateGhostBehaviour(Ghost pGhost)
	{
		if (pGhost.IsDead())
			return;

		pGhost.m_pNavMeshAgent.SetDestination(PlayerCharacter.Instance.transform.position);

		if (!pGhost.m_pProceduralVariablesModule.GetVariableAsBool(c_sVariableName_bIsEnraged))
			CheckIfShouldEnrage(pGhost);
	}

	private void CheckIfShouldEnrage(Ghost pGhost)
	{
		int iCurrentlyCollectedPellets = PlayerCharacter.Instance.m_pPickupsModule.GetCurrentPelletsCount();
		int iCollectedPelletsAtBehaviourStart = (int)(pGhost.m_pProceduralVariablesModule.GetVariable(c_sVariableName_iPelletsCollectedByPlayerAtBehaviourStart, 0));

		if (iCurrentlyCollectedPellets - iCollectedPelletsAtBehaviourStart >= m_iPelletsCollectedCountToEnrage)
			EnrageGhost(pGhost);
	}

	private void EnrageGhost(Ghost pGhost)
	{
		pGhost.m_pProceduralVariablesModule.SetVariable(c_sVariableName_bIsEnraged, true);
		pGhost.m_pNavMeshAgent.speed = pGhost.m_fRegularSpeed * m_fEnrageSpeedMultiplier;
	}
}

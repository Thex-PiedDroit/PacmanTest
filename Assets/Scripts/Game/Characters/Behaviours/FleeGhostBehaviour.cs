
using UnityEngine;
using UnityEngine.AI;


[CreateAssetMenu(fileName = "FleeGhost", menuName = "GhostsBehaviours/Flee")]
public class FleeGhostBehaviour : GhostBehaviour
{
#region Variables (public)

	public float m_fSpeedMultiplier = 0.6f;
	public float m_fSuperPelletEndWarningBlinksPerSecond = 2.0f;

	#endregion

#region Variables (private)

	static private float s_fTimeWarningStarted = 0.0f;

	private const string c_sVariableName_pTileLastFrame = "pTileLastFrame";
	private const string c_sVariableName_pTileWeJustLeft = "pTileWeJustLeft";

	private const string c_sVariableName_bReceivedBehaviourWhileInPrison = "bReceivedFleeBehaviourWhileInPrison";
	private const string c_sVariableName_bFleeBehaviourWentOnPrisonDoor = "bFleeBehaviourWentOnPrisonDoor";

	#endregion


	override public void InitBehaviour(Ghost pGhost)
	{
		base.InitBehaviour(pGhost);

		pGhost.m_pNavMeshAgent.speed = pGhost.m_fRegularSpeed * m_fSpeedMultiplier;
		s_fTimeWarningStarted = 0.0f;

		InitVariables(pGhost);
	}

	/// <summary>
	/// Public so it can be used by other behaviours if needed (NOTHING ELSE!)
	/// </summary>
	public void InitVariables(Ghost pGhost)
	{
		CharacterProceduralVariablesModule pVariablesModule = pGhost.m_pProceduralVariablesModule;

		pVariablesModule.ResetVariable(c_sVariableName_pTileLastFrame);
		pVariablesModule.ResetVariable(c_sVariableName_pTileWeJustLeft);

		pVariablesModule.SetVariable(c_sVariableName_bReceivedBehaviourWhileInPrison, pGhost.IsDead());
		pVariablesModule.ResetVariable(c_sVariableName_bFleeBehaviourWentOnPrisonDoor);
	}

	override public void UpdateGhostBehaviour(Ghost pGhost)
	{
		if (!pGhost.IsDead())
			UpdateFleeMovement(pGhost);

		if (GameManager.Instance.SuperPelletEffectAboutToWearOut)
			UpdateEffectEndWarning(pGhost);
	}

	/// <summary>
	/// Public so it can be used by other behaviours if needed (NOTHING ELSE!)
	/// </summary>
	public void UpdateFleeMovement(Ghost pGhost)
	{
		CharacterProceduralVariablesModule pVariablesModule = pGhost.m_pProceduralVariablesModule;

		if (pVariablesModule.GetVariableAsBool(c_sVariableName_bReceivedBehaviourWhileInPrison))
		{
			GetOutOfPrison(pGhost);
			return;
		}

		Tile pCurrentTile = MapManager.Instance.GetTileFromPosition(pGhost.transform.position);
		UpdateSavedTiles(pGhost, pCurrentTile);

		Tile pTileWeJustLeft = (Tile)(pVariablesModule.GetVariable(c_sVariableName_pTileWeJustLeft));

		Tile pTileFurtherFromPlayer = MapManager.Instance.GetAdjacentTileFurtherFromPosition(PlayerCharacter.Instance.transform.position, pCurrentTile, pTileWeJustLeft);
		if (pTileFurtherFromPlayer != null)
			pGhost.m_pNavMeshAgent.SetDestination(pTileFurtherFromPlayer.transform.position);
		else
			pVariablesModule.SetVariable(c_sVariableName_pTileWeJustLeft, pCurrentTile);     // Since there's no valid adjacent tile, don't exclude last tile anymore, cause it might become the best option
	}

	private void GetOutOfPrison(Ghost pGhost)
	{
		CharacterProceduralVariablesModule pVariablesModule = pGhost.m_pProceduralVariablesModule;

		bool bWentOnPrisonDoor = pVariablesModule.GetVariableAsBool(c_sVariableName_bFleeBehaviourWentOnPrisonDoor);
		bool bCurrentTileIsGhostDoor = MapManager.Instance.GetTileFromPosition(pGhost.transform.position).TileType == ETileType.GHOST_DOOR;

		if (!bWentOnPrisonDoor)
			pVariablesModule.SetVariable(c_sVariableName_bFleeBehaviourWentOnPrisonDoor, bCurrentTileIsGhostDoor);
		else if (!bCurrentTileIsGhostDoor)
			pVariablesModule.SetVariable(c_sVariableName_bReceivedBehaviourWhileInPrison, false);

		pGhost.m_pNavMeshAgent.SetDestination(PlayerCharacter.Instance.transform.position);
	}

	private void UpdateSavedTiles(Ghost pGhost, Tile pCurrentTile)
	{
		CharacterProceduralVariablesModule pVariablesModule = pGhost.m_pProceduralVariablesModule;

		Tile pTileLastFrame = (Tile)(pVariablesModule.GetVariable(c_sVariableName_pTileLastFrame));
		if (pCurrentTile != pTileLastFrame)
			pVariablesModule.SetVariable(c_sVariableName_pTileWeJustLeft, pTileLastFrame);

		pVariablesModule.SetVariable(c_sVariableName_pTileLastFrame, pCurrentTile);
	}

	private void UpdateEffectEndWarning(Ghost pGhost)
	{
		if (s_fTimeWarningStarted == 0.0f)
			s_fTimeWarningStarted = Time.time;

		float fTimeElapsed = Time.time - s_fTimeWarningStarted;
		Color tColor = Mathf.Sin(fTimeElapsed * Mathf.PI * (m_fSuperPelletEndWarningBlinksPerSecond * 2.0f)) >= 0.0f ? Color.white : m_tGhostColor;

		pGhost.SetGhostColor(tColor);
	}
}

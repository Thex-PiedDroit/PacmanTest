﻿
using UnityEngine;


[CreateAssetMenu(fileName = "FleeGhostBehaviour", menuName = "GhostsBehaviours/Flee")]
public class FleeGhostBehaviour : GhostBehaviour
{
#region Variables (public)

	public float m_fSpeedMultiplier = 0.6f;
	public float m_fSuperPelletEndWarningBlinksPerSecond = 2.0f;

	#endregion

#region Variables (private)

	static private float s_fTimeWarningStarted = 0.0f;

	#endregion


	override public void InitBehaviour(Ghost pGhost)
	{
		base.InitBehaviour(pGhost);

		pGhost.m_pNavMeshAgent.speed = pGhost.m_fRegularSpeed * m_fSpeedMultiplier;
		s_fTimeWarningStarted = 0.0f;
	}

	override public void UpdateGhostBehaviour(Ghost pGhost)
	{
		if (!pGhost.IsDead())
		{
			Vector3 tPosFurtherToPlayer = MapManager.Instance.GetPosOnMapFurtherFromPosition(PlayerCharacter.Instance.transform.position);  // TODO: Find a better way to flee, this is a bit lackluster
			pGhost.m_pNavMeshAgent.SetDestination(tPosFurtherToPlayer);
		}

		if (GameManager.Instance.SuperPelletEffectAboutToWearOut)
			UpdateEffectEndWarning(pGhost);
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

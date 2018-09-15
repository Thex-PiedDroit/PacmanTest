﻿
using UnityEngine;


abstract public class GhostBehaviour : ScriptableObject
{
	public Color m_tGhostColor = Color.red;


	/// <summary>
	/// Should be called by children!
	/// </summary>
	virtual public void InitBehaviour(Ghost pGhost)
	{
		pGhost.SetGhostColor(m_tGhostColor);
		pGhost.m_pNavMeshAgent.speed = pGhost.m_fRegularSpeed;
	}

	abstract public void UpdateGhostBehaviour(Ghost pGhost);
}

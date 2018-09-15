
using UnityEngine;
using UnityEngine.AI;


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

	/// <summary>
	/// Empty by default
	/// </summary>
	virtual public void ResetGhostBehaviour(Ghost pGhost)
	{

	}

	abstract public void UpdateGhostBehaviour(Ghost pGhost);
}

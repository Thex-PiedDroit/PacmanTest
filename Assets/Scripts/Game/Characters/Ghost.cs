
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;


public class Ghost : MonoBehaviour
{
#region Variables (public)

	public Action OnDeath = null;

	public NavMeshAgent m_pNavMeshAgent = null;
	public MeshFilter m_pMeshFilter = null;
	public CharacterProceduralVariablesModule m_pProceduralVariablesModule = null;

	public GhostBehaviour m_pBehaviour = null;

	public float m_fRegularSpeed = 4.8f;

	#endregion

#region Variables (private)

	private bool m_bAlive = false;

	#endregion


	public void GiveBehaviour(GhostBehaviour pBehaviour)
	{
		m_pBehaviour = pBehaviour;
		pBehaviour.InitBehaviour(this);
	}

	private void Update()
	{
		if (m_bAlive && m_pBehaviour != null)
			m_pBehaviour.UpdateGhostBehaviour(this);
	}

	public void SetGhostColor(Color tGhostColor)
	{
		int iVerticesCount = m_pMeshFilter.mesh.vertices.Length;
		Color[] pColors = new Color[iVerticesCount];

		for (int i = 0; i < iVerticesCount; ++i)
			pColors[i] = tGhostColor;

		m_pMeshFilter.mesh.colors = pColors;
	}

	public bool IsDead()
	{
		return !m_bAlive;
	}

	public void KillGhost()
	{
		m_bAlive = false;
		OnDeath?.Invoke();
	}

	/// <summary>
	/// Use this instead of KillGhost() to avoid counting it as a death (to disable its behaviour upon respawn for example)
	/// </summary>
	public void SetDead()
	{
		m_bAlive = false;
		m_pNavMeshAgent.enabled = false;
		m_pProceduralVariablesModule.ResetVariables();
	}

	public void SetAlive()
	{
		m_bAlive = true;
		m_pNavMeshAgent.enabled = true;
		m_pBehaviour.ResetGhostBehaviour(this);
	}

	private void OnTriggerEnter(Collider pOther)
	{
		PlayerCharacter pPlayer = pOther.GetComponent<PlayerCharacter>();
		Assert.IsTrue(pPlayer != null, "A ghost has collided with something that is not the player character (" + pOther.name + "). Please make sure the layers and collisions are correctly setup.");

		pPlayer.KillPlayer();
	}
}

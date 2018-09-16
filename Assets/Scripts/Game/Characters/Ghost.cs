
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;


public class Ghost : MonoBehaviour
{
#region Variables (public)

	/// <summary>
	/// Ghost is this
	/// </summary>
	public Action<Ghost> OnDeath = null;

	public NavMeshAgent m_pNavMeshAgent = null;
	public MeshFilter m_pMeshFilter = null;
	public CharacterProceduralVariablesModule m_pProceduralVariablesModule = null;

	public Rigidbody m_pRigidbody = null;
	public Color m_tDeadColorMultiplier = Color.grey;

	public GhostBehaviour m_pBehaviour = null;

	public string m_sAliveLayerName = "Ghost";
	public string m_sDeadLayerName = "DeadGhost";

	public float m_fRegularSpeed = 4.8f;

	#endregion

#region Variables (private)

	private Tile m_pSpawnTile = null;

	private Color m_tCurrentColor = Color.white;

	private bool m_bAlive = false;
	private bool m_bRagdollActive = false;

	private bool m_bBehaviourFrozen = false;

	#endregion


	public void GiveBehaviour(GhostBehaviour pBehaviour)
	{
		m_pBehaviour = pBehaviour;
		pBehaviour.InitBehaviour(this);
	}

	private void Update()
	{
		if (m_pBehaviour != null && !m_bBehaviourFrozen)
			m_pBehaviour.UpdateGhostBehaviour(this);
	}

	public void SetGhostColor(Color tGhostColor)
	{
		if (m_bRagdollActive)
			tGhostColor *= m_tDeadColorMultiplier;

		if (tGhostColor == m_tCurrentColor)
			return;

		m_tCurrentColor = tGhostColor;

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
		SetDead();
		SetRagdollModeActivated(true);

		OnDeath?.Invoke(this);

		PlayerCharacter.Instance.TriggerPlayerKilledGhost(this);
	}

	/// <summary>
	/// Use this instead of KillGhost() to avoid counting it as a death (to disable its behaviour upon respawn for example)
	/// </summary>
	public void SetDead()
	{
		m_bAlive = false;
		m_pNavMeshAgent.enabled = false;
		SetBehaviourFrozen(false);
	}

	public void SetAlive()
	{
		SetRagdollModeActivated(false);

		m_bAlive = true;
		transform.position = m_pSpawnTile.transform.position;

		m_pNavMeshAgent.enabled = true;
		m_pBehaviour.ResetGhostBehaviour(this);
	}

	public void SetSpawnTile(Tile pTile)
	{
		m_pSpawnTile = pTile;
	}

	private void SetRagdollModeActivated(bool bActivated)
	{
		m_bRagdollActive = bActivated;
		SetGhostColor(m_pBehaviour.m_tGhostColor);

		m_pRigidbody.isKinematic = !bActivated;
		m_pRigidbody.useGravity = bActivated;

		gameObject.layer = LayerMask.NameToLayer(bActivated ? m_sDeadLayerName : m_sAliveLayerName);

		if (!bActivated)
			transform.rotation = Quaternion.identity;
	}

	public void SetBehaviourFrozen(bool bFrozen)
	{
		m_bBehaviourFrozen = bFrozen;
		m_pNavMeshAgent.enabled = !bFrozen;
	}

	private void OnTriggerEnter(Collider pOther)
	{
		if (!m_bAlive || pOther.gameObject.layer == LayerMask.NameToLayer("Ground") || pOther.gameObject.layer == LayerMask.NameToLayer("Wall"))
			return;

		PlayerCharacter pPlayer = pOther.GetComponent<PlayerCharacter>();
		Assert.IsTrue(pPlayer != null, "A ghost has collided with something that is not the player character (" + pOther.name + "). Please make sure the layers and collisions are correctly setup.");

		if (!pPlayer.CanKillGhosts)
			pPlayer.KillPlayer();
		else
			KillGhost();
	}
}

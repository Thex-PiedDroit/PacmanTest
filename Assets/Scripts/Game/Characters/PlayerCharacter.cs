
using UnityEngine;


public class PlayerCharacter : MonoBehaviour
{
#region Variables (public)

	public PlayerCharacterBehaviour m_pPlayerCharacterBehaviour = null;
	public float m_fMoveSpeed = 5.0f;

	#endregion

#region Variables (private)

	private Tile m_pCurrentTileTarget = null;	// Character will find the next one whenever reaching its target
	private Tile m_pNextTileFromInputs = null;	// If this is not null upon reaching the current target, and it is adjacent, this will become the new target. Otherwise, we will get closer before setting it as a target

	private float m_fLastFrameMovementOvershoot = 0.0f;

	#endregion


	public void SetSpawnTile(Tile pTile)
	{
		transform.position = pTile.transform.position;
	}

	private void Update()
	{
		m_pPlayerCharacterBehaviour.UpdateCharacterTileTargets(this);

		if (m_pCurrentTileTarget != null)
			MoveTowardsCurrentTileTarget();
	}

	private void MoveTowardsCurrentTileTarget()
	{
		Vector3 tMoveThisFrame = transform.forward * ((m_fMoveSpeed * Time.deltaTime) + m_fLastFrameMovementOvershoot);
		m_fLastFrameMovementOvershoot = 0.0f;

		Vector3 tMeToDestination = transform.position - m_pCurrentTileTarget.transform.position;

		if (tMeToDestination.sqrMagnitude <= tMoveThisFrame.sqrMagnitude)
		{
			m_fLastFrameMovementOvershoot = (tMoveThisFrame - tMeToDestination).magnitude;

			transform.position = m_pCurrentTileTarget.transform.position;

			if (m_pNextTileFromInputs != null && m_pNextTileFromInputs.IsAdjacentTo(m_pCurrentTileTarget))
			{
				m_pCurrentTileTarget = m_pNextTileFromInputs;
				m_pNextTileFromInputs = null;
			}
			else
			{
				m_pCurrentTileTarget = m_pPlayerCharacterBehaviour.FindWalkableTileInDirection(transform.forward, transform.position);
			}

			if (m_pCurrentTileTarget != null)
				transform.forward = (m_pCurrentTileTarget.transform.position - transform.position).normalized;
			else
				m_fLastFrameMovementOvershoot = 0.0f;
		}
		else
		{
			transform.position += tMoveThisFrame;
		}
	}

	public void ForceSetCurrentTileTarget(Tile pTile)
	{
		m_pCurrentTileTarget = pTile;
		m_pNextTileFromInputs = null;

		if (m_pCurrentTileTarget != null)
			transform.forward = (pTile.transform.position - transform.position).normalized;
	}

	public void SetNextTileTarget(Tile pTile)
	{
		if (m_pCurrentTileTarget == null)
			ForceSetCurrentTileTarget(pTile);
		else
			m_pNextTileFromInputs = pTile;
	}

	public Vector3 GetCurrentTargetPosition()
	{
		return m_pCurrentTileTarget?.transform.position ?? transform.position;
	}
}

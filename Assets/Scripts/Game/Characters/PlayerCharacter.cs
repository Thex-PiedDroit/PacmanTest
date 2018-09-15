
using UnityEngine;


public class PlayerCharacter : MonoBehaviour
{
#region Variables (public)

	public PlayerCharacterBehaviour m_pPlayerCharacterBehaviour = null;

	public PlayerPickupsModule m_pPickupsModule = null;

	public float m_fMoveSpeed = 5.0f;

	#endregion

#region Variables (private)

	private Tile m_pCurrentTileTarget = null;	// Character will find the next one whenever reaching its target
	private Tile m_pInputsTileTarget = null;	// If this is not null upon reaching the current target, and it is adjacent, this will become the new target. Otherwise, we will get closer before setting it as a target
												// This system makes it possible to gracefully handle diagonal inputs while keeping movements non-diagonal. In other words: more complicated in order to feel better
	private float m_fLastFrameMovementOvershoot = 0.0f;

	#endregion


#region Behaviour

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

			if (m_pCurrentTileTarget.TileType == ETileType.WARP)
				transform.position = m_pCurrentTileTarget.GetWarpPosition();
			else
				transform.position = m_pCurrentTileTarget.transform.position;

			AcquireNextTileTargetInCurrentDirection();
		}
		else
		{
			transform.position += tMoveThisFrame;
		}
	}

	#endregion

#region Behaviour Helpers

	private void AcquireNextTileTargetInCurrentDirection()
	{
		Tile pNextTileTarget = null;

		if (m_pInputsTileTarget != null)
		{
			pNextTileTarget = FindTileToReachInputsOne();

			if (pNextTileTarget == m_pInputsTileTarget)
				ClearInputsTileTarget();
		}
		else
		{
			pNextTileTarget = m_pPlayerCharacterBehaviour.FindWalkableTileInDirection(transform.forward, transform.position);
		}

		SetCurrentTileTarget(pNextTileTarget);
	}

	/// <summary>
	/// Looks to the tiles adjacent both to our current one and the inputs target to find which one is walkable
	/// </summary>
	private Tile FindAdjacentTileClosestToInputsTarget()
	{
		Vector3 tInputsTargetPos = m_pInputsTileTarget.transform.position;
		Tile pTileOnMyXAxis = MapManager.Instance.GetTileFromPosition(transform.position.x, tInputsTargetPos.z);

		if (pTileOnMyXAxis.IsWalkable())
			return pTileOnMyXAxis;

		return MapManager.Instance.GetTileFromPosition(tInputsTargetPos.x, transform.position.z);
	}

	/// <summary>
	/// Tries to find a tile that will get us closer to the inputs target, if needed.
	/// If the inputs target is already adjacent to our current one, will return it instead (don't forget to call ClearInputsTarget() if you use it).
	/// </summary>
	private Tile FindTileToReachInputsOne()
	{
		Tile pTileTarget = null;

		if (m_pInputsTileTarget.IsAdjacentTo(MapManager.Instance.GetTileFromPosition(transform.position.x, transform.position.z)))
			pTileTarget = m_pInputsTileTarget;		// If it's already adjacent to where we are, directly go there
		else
			pTileTarget = FindAdjacentTileClosestToInputsTarget();

		return pTileTarget;
	}

	#endregion


#region Setters

	public void SetSpawnTile(Tile pTile)
	{
		transform.position = pTile.transform.position;
	}

	public void SetCurrentTileTarget(Tile pTile)
	{
		m_pCurrentTileTarget = pTile;

		if (pTile != null)
			transform.forward = (pTile.transform.position - transform.position).normalized;
		else
			m_fLastFrameMovementOvershoot = 0.0f;
	}

	public void SetInputsTileTarget(Tile pTile)
	{
		m_pInputsTileTarget = pTile;

		if (m_pCurrentTileTarget == null)
			SetCurrentTileTarget(FindTileToReachInputsOne());
	}

	public void ClearInputsTileTarget()
	{
		m_pInputsTileTarget = null;
	}

	#endregion

#region Getters

	public Vector3 GetCurrentTargetPosition()
	{
		return m_pCurrentTileTarget?.transform.position ?? transform.position;
	}

	#endregion
}

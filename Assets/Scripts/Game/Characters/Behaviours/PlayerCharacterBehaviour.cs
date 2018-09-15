
using UnityEngine;
using UnityEngine.AI;


[CreateAssetMenu()]
public class PlayerCharacterBehaviour : ScriptableObject
{
	public void UpdateCharacterTileTargets(PlayerCharacter pCharacter)
	{
		Vector3 tMoveDirection = QueryMoveInputs();
		Vector3 tCurrentCharacterForward = pCharacter.transform.forward;

		if (tMoveDirection == Vector3.zero)
		{
			pCharacter.ClearInputsTileTarget();
			return;
		}

		if (tMoveDirection == -tCurrentCharacterForward || pCharacter.HasntStartedMovingYet())	// "Abort current tile target, go back (therefore, directly set a new target)
		{
			Tile pNextTileInDirection = FindWalkableTileInDirection(tMoveDirection, pCharacter.transform.position - (tMoveDirection * 0.5f));  // Raycast from a bit behind the character to find the one under him if he just got passed its middle but didn't leave it yet

			if (pNextTileInDirection != null)
			{
				pCharacter.SetCurrentTileTarget(pNextTileInDirection);
				pCharacter.ClearInputsTileTarget();
			}
		}
		else												// Suggest a next direction for when the current tile target has been reached
		{
			Vector3 tCurrentTargetPosition = pCharacter.GetCurrentTargetPosition();

			Tile pNextTileInDirection = FindWalkableTileInDirection(tMoveDirection, tCurrentTargetPosition - (pCharacter.transform.forward * 0.4f));	// Raycasting from a bit behind the center of the tile (but still in it) to avoid getting stuck facing walls, if there's a perpendicular path
			if (pNextTileInDirection != null && (pNextTileInDirection.transform.position - tCurrentTargetPosition).normalized != tCurrentCharacterForward)
				pCharacter.SetInputsTileTarget(pNextTileInDirection);
		}
	}

	private Vector3 QueryMoveInputs()
	{
		float fHorizontal = Input.GetAxis("Horizontal");
		float fVertical = Input.GetAxis("Vertical");

		Vector3 tInputVector = new Vector3(fHorizontal, 0.0f, fVertical);

		return tInputVector.sqrMagnitude > 0.0025f ? tInputVector.normalized : Vector3.zero;
	}

	public Tile FindWalkableTileInDirection(Vector3 tDirection, Vector3 tFromPosition)
	{
		const float c_fDistanceInFrontOfPositionToRaycastFrom = 1.1f;	// Slightly more than 1.0f in order to be sure to reach the next tile even from the very beginning of one
		Vector3 tRaycastOrigin = tFromPosition + (tDirection * c_fDistanceInFrontOfPositionToRaycastFrom) + (Vector3.up * 2.0f);	// We raycast up to down one tile in front of the character

		RaycastHit tHit;
		if (!Physics.Raycast(tRaycastOrigin, Vector3.down, out tHit, 2.0f, LayerMask.GetMask("Tile"), QueryTriggerInteraction.Collide))
			return null;

		Tile pTile = tHit.collider.GetComponent<Tile>();
		if (!pTile.IsWalkable())
			pTile = null;

		return pTile;
	}
}

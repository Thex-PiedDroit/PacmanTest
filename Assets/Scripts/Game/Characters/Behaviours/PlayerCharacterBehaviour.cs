﻿
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

		if (tMoveDirection == -tCurrentCharacterForward)	// "Abort current tile target, go back (therefore, directly set a new target)
		{
			Tile pNextTileInDirection = FindWalkableTileInDirection(tMoveDirection, pCharacter.transform.position - (tMoveDirection * 0.5f));  // Raycast from a bit behind the character to find the one under him if he just got passed its middle but didn't leave it yet
			pCharacter.SetCurrentTileTarget(pNextTileInDirection);
			pCharacter.ClearInputsTileTarget();
		}
		else												// Suggest a next direction for when the current tile target has been reached
		{
			Vector3 tCurrentTargetPosition = pCharacter.GetCurrentTargetPosition();

			Tile pNextTileInDirection = FindWalkableTileInDirection(tMoveDirection, tCurrentTargetPosition - (pCharacter.transform.forward * 0.4f));
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
		RaycastHit tHit;
		if (!Physics.Raycast(tFromPosition + (tDirection * 1.1f) + (Vector3.up * 2.0f), Vector3.down, out tHit, 2.0f, LayerMask.GetMask("Tile"), QueryTriggerInteraction.Collide))
			return null;

		Tile pTile = tHit.collider.GetComponent<Tile>();
		if (!pTile.IsWalkable())
			pTile = null;

		return pTile;
	}
}

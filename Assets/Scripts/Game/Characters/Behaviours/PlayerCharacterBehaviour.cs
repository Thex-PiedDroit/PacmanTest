
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
			Tile pNextTileInDirection = MapManager.Instance.GetWalkableTileInDirection(tMoveDirection, pCharacter.transform.position - (tMoveDirection * 0.5f));  // Checking from a bit behind the character to find the one under him if he just got passed its middle but didn't leave it yet

			if (pNextTileInDirection != null)
			{
				pCharacter.SetCurrentTileTarget(pNextTileInDirection);
				pCharacter.ClearInputsTileTarget();
			}
		}
		else												// Suggest a next direction for when the current tile target has been reached
		{
			Vector3 tCurrentTargetPosition = pCharacter.GetCurrentTargetPosition();

			Tile pNextTileInDirection = MapManager.Instance.GetWalkableTileInDirection(tMoveDirection, tCurrentTargetPosition - (pCharacter.transform.forward * 0.4f));	// Checking from a bit behind the center of the tile (but still in it) to avoid getting stuck facing walls, if there's a perpendicular path
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
}

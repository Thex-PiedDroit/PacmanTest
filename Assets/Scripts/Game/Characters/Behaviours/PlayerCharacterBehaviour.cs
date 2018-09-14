
using UnityEngine;
using UnityEngine.AI;


[CreateAssetMenu()]
public class PlayerCharacterBehaviour : CharacterBehaviour
{
	public float m_fDistanceToCheckForTilesFromInput = 2.0f;


	public override void UpdateCharacter(Character pCharacter)
	{
		Vector3 tInputDirection = QueryMoveInputs();
		Tile pTileInDirection = null;

		if (tInputDirection != Vector3.zero)
		{
			pTileInDirection = FindTileInDirection(tInputDirection, pCharacter);

			if (pTileInDirection.transform.position != pCharacter.m_pNavMeshAgent.destination)
			{
				pCharacter.m_pNavMeshAgent.SetDestination(pTileInDirection.transform.position);
				return;
			}
		}

		pTileInDirection = FindTileInDirection(pCharacter.transform.forward, pCharacter);
		pCharacter.m_pNavMeshAgent.SetDestination(pTileInDirection.transform.position);
	}

	private Vector3 QueryMoveInputs()
	{
		float fHorizontal = Input.GetAxis("Horizontal");
		float fVertical = Input.GetAxis("Vertical");

		return new Vector3(fHorizontal, 0.0f, fVertical);
	}

	private Tile FindTileInDirection(Vector3 tDirection, Character pFromCharacter)
	{
		Vector3 tPositionInDirection = pFromCharacter.transform.position + (tDirection.normalized * m_fDistanceToCheckForTilesFromInput);

		NavMeshHit tHit;
		NavMesh.SamplePosition(tPositionInDirection, out tHit, float.MaxValue, 1);		// TODO: Kind of an overkill at this point, although it allows for graceful handling of perfectly diagonal input; should simplify this at a later point

		RaycastHit tTileHit;
		Physics.Raycast(tHit.position + (Vector3.up * 2.0f), Vector3.down, out tTileHit, 3.0f, LayerMask.GetMask("Tile"), QueryTriggerInteraction.Collide);

		Tile pTile = tTileHit.collider.GetComponent<Tile>();
		if (!pTile.IsWalkable())
			pTile = null;

		return pTile;
	}
}

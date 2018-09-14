
using UnityEngine;
using UnityEngine.AI;


[CreateAssetMenu()]
public class PlayerCharacterBehaviour : CharacterBehaviour
{
	public float m_fDistanceToCheckForWalls = 0.75f;


	public override void UpdateCharacterDestination(Character pCharacter)
	{
		Vector3 tMoveDirection = QueryMoveInputs();

		if (tMoveDirection.sqrMagnitude <= 0.0025f || !CanGoInThatDirection(tMoveDirection, pCharacter))
			tMoveDirection = pCharacter.transform.forward;

		tMoveDirection = MakeDirectionOnOneAxisOnly(tMoveDirection, pCharacter.transform.forward);

		pCharacter.m_pNavMeshAgent.SetDestination(pCharacter.transform.position + (tMoveDirection * m_fDistanceToCheckForWalls));
		pCharacter.transform.forward = tMoveDirection;
	}

	private Vector3 QueryMoveInputs()
	{
		float fHorizontal = Input.GetAxis("Horizontal");
		float fVertical = Input.GetAxis("Vertical");

		return new Vector3(fHorizontal, 0.0f, fVertical);
	}

	/// <summary>
	/// Will remove one of X or Z, whichever smaller, to be a one-axis only direction. If both are equal, will make the direction perpendicular to current character's direction
	/// </summary>
	private Vector3 MakeDirectionOnOneAxisOnly(Vector3 tDirection, Vector3 tCurrentCharacterDirection)
	{
		if ((tDirection.x - tDirection.z).Sqrd() <= 0.025f)
		{
			if (tCurrentCharacterDirection.x.Sqrd() > tCurrentCharacterDirection.z.Sqrd())
			{
				tDirection.x = 0.0f;
			}
			else
			{
				tDirection.z = 0.0f;
			}
		}
		else if (tDirection.x.Sqrd() > tDirection.z.Sqrd())
		{
			tDirection.z = 0.0f;
		}
		else
		{
			tDirection.x = 0.0f;
		}

		return tDirection.normalized;
	}

	private bool CanGoInThatDirection(Vector3 tDirection, Character pFromCharacter)
	{
		NavMeshHit tHit;
		return !NavMesh.Raycast(pFromCharacter.transform.position, pFromCharacter.transform.position + (tDirection.normalized * m_fDistanceToCheckForWalls), out tHit, NavMesh.AllAreas);
	}
}

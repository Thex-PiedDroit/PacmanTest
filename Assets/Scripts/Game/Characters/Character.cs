
using UnityEngine;
using UnityEngine.AI;


public class Character : MonoBehaviour
{
#region Variables (public)

	public NavMeshAgent m_pNavMeshAgent = null;

	public CharacterBehaviour m_pBehaviour = null;

	#endregion

#region Variables (private)



	#endregion


	private void Update()
	{
		m_pBehaviour.UpdateCharacter(this);
	}
}

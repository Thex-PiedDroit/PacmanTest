
using UnityEngine;
using UnityEngine.AI;


public class Ghost : MonoBehaviour
{
#region Variables (public)

	public NavMeshAgent m_pNavMeshAgent = null;

	public MeshFilter m_pMeshFilter = null;

	public GhostBehaviour m_pBehaviour = null;

	public float m_fRegularSpeed = 4.8f;

	#endregion

#region Variables (private)



	#endregion


	private void Start()
	{
		SetGhostColor(m_pBehaviour.m_tGhostColor);
		m_pNavMeshAgent.speed = m_fRegularSpeed;
	}

	private void Update()
	{
		m_pBehaviour.UpdateGhostDestination(this);
	}

	private void SetGhostColor(Color tGhostColor)
	{
		int iVerticesCount = m_pMeshFilter.mesh.vertices.Length;
		Color[] pColors = new Color[iVerticesCount];

		for (int i = 0; i < iVerticesCount; ++i)
			pColors[i] = tGhostColor;

		m_pMeshFilter.mesh.colors = pColors;
	}
}


using UnityEngine;


public class GrapplingHook : MonoBehaviour
{
#region Variables (public)

	public GameObject m_pRopeObject = null;

	public LineRenderer m_pLineRenderer = null;
	public Transform m_pRopeBegin = null;
	public Transform m_pRopeEnd = null;

	#endregion

#region Variables (private)



	#endregion


	private void LateUpdate()
	{
		if (m_pRopeObject.activeSelf)
			UpdateRopeEnds();
	}

	private void UpdateRopeEnds()
	{
		m_pLineRenderer.SetPosition(0, m_pRopeBegin.position);
		m_pLineRenderer.SetPosition(1, m_pRopeEnd.position);
	}
}

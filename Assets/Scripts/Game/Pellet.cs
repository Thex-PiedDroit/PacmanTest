
using UnityEngine;


public class Pellet : MonoBehaviour
{
#region Variables (public)

	public float m_fSuperPelletScale = 1.0f;
	public float m_fRegularPelletScale = 0.5f;

	#endregion

#region Variables (private)

	private bool m_bIsSuperPellet = false;

	#endregion


	public void InitPellet(bool bIsSuperPellet)
	{
		m_bIsSuperPellet = bIsSuperPellet;
		transform.localScale = Vector3.one * (bIsSuperPellet ? m_fSuperPelletScale : m_fRegularPelletScale);
	}
}

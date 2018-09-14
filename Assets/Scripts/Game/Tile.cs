
using UnityEngine;


public enum ETileType
{
	WALL			= 'X',
	PELLET			= '.',
	SUPER_PELLET	= 'o',
	WARP			= '=',
	GHOST_DOOR		= '-',
	GHOST_SPAWNER	= 'G',
	BONUS_SPAWNER	= 'B',
	EMPTY			= ' '
}


public class Tile : MonoBehaviour
{
#region Variables (public)

	public GameObject m_pWallObject = null;
	public Pellet m_pPellet = null;

	#endregion

#region Variables (private)

	private ETileType m_eTileType = ETileType.EMPTY;

	#endregion


	public void SetTileType(ETileType eTileType)
	{
		m_eTileType = eTileType;
		
		m_pWallObject.SetActive(eTileType == ETileType.WALL);
		InitPellet();
	}

	private void InitPellet()
	{
		if (m_eTileType != ETileType.PELLET && m_eTileType != ETileType.SUPER_PELLET)
		{
			m_pPellet.gameObject.SetActive(false);
			return;
		}

		m_pPellet.gameObject.SetActive(true);
		m_pPellet.InitPellet(m_eTileType == ETileType.SUPER_PELLET);
	}
}

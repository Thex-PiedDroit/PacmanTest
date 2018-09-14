
using UnityEngine;


public enum ETileType
{
	WALL			= 'X',
	PELLET			= '.',
	SUPER_PELLET	= 'o',
	WARP			= '=',
	GHOST_DOOR		= '-',
	GHOST_SPAWNER	= 'G',
	PACMAN_SPAWNER	= 'P',
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
	public ETileType TileType { get { return m_eTileType; } }

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

	public bool IsAdjacentTo(Tile pTile)
	{
		bool bIsAdjacentOnX = (Mathf.Abs(transform.position.x) - Mathf.Abs(pTile.transform.position.x)).Sqrd() == 1.0f;
		bool bIsAdjacentOnY = (Mathf.Abs(transform.position.z) - Mathf.Abs(pTile.transform.position.z)).Sqrd() == 1.0f;

		return bIsAdjacentOnX ^ bIsAdjacentOnY;
	}

	public bool IsWalkable()
	{
		bool bIsWalkable = true;

		switch (m_eTileType)
		{
			case ETileType.WALL:
			case ETileType.GHOST_DOOR:
			case ETileType.GHOST_SPAWNER:
				bIsWalkable = false;
				break;
		}

		return bIsWalkable;
	}
}

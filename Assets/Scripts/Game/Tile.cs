
using UnityEngine;
using UnityEngine.Assertions;


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

	private Tile m_pConnectedPortal = null;

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

	public void SetConnectedPortal(Tile pTile)
	{
		m_pConnectedPortal = pTile;
	}

	public Vector3 GetWarpPosition()
	{
		Assert.IsTrue(m_pConnectedPortal != null, "Someone tried to get a warp position from a tile which is a " + m_eTileType + " that has no connected portal");
		return m_pConnectedPortal.transform.position;
	}

	public bool IsAdjacentTo(Tile pTile)
	{
		bool bIsTrue = (transform.position - pTile.transform.position).sqrMagnitude == 1.0f;
		return bIsTrue;
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

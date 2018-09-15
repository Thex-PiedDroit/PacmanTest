
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using System.Collections.Generic;


public class MapManager : MonoBehaviour
{
#region Variables (public)

	static public MapManager Instance = null;

	public float m_fTilesSize = 1.0f;

	public Tile m_pTilePrefab = null;
	public PelletEffect m_pRegularPelletsEffect = null;
	public SuperPelletEffect m_pSuperPelletsEffect = null;

	public GameObject m_pGround = null;

	public List<Transform> m_pPortalVisuals = null;

	public List<Tile> m_pTiles = null;

	#endregion

#region Variables (private)

	private int m_iGridSizeX = 0;
	private int m_iGridSizeY = 0;

	private Tile m_pUnconnectedPortal = null;
	private int m_iPlacedPortalVisuals = 0;

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;
	}

	public void LoadMap(string sMapName)
	{
		List<char> pTilesTypes = ExtractMapDataFromString(sMapName);
		HideExcessTiles();

		m_pUnconnectedPortal = null;
		m_iPlacedPortalVisuals = 0;


		m_pGround.transform.localScale = new Vector3(m_iGridSizeX, m_pGround.transform.localScale.y, m_iGridSizeY);

		Vector2 tHalfMapSize = GetHalfMapSize();

		for (int i = 0; i < pTilesTypes.Count; ++i)
		{
			int iX = i % m_iGridSizeX;
			int iY = i / m_iGridSizeX;

			float fPosX = (iX - tHalfMapSize.x) * m_fTilesSize;
			float fPosY = (-iY + tHalfMapSize.y) * m_fTilesSize;

			Tile pCurrentTile = null;

			if (m_pTiles.Count <= i)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					pCurrentTile = PrefabUtility.InstantiatePrefab(m_pTilePrefab) as Tile;
					pCurrentTile.transform.parent = transform;
				}
				else
#endif
				{
					pCurrentTile = Instantiate(m_pTiles[0], transform);
				}

				m_pTiles.Add(pCurrentTile);
			}
			else
			{
				pCurrentTile = m_pTiles[i];
				pCurrentTile.gameObject.SetActive(true);
			}

			pCurrentTile.transform.localPosition = new Vector3(fPosX, 0.0f, fPosY);

			InitTileWithType(pCurrentTile, (ETileType)pTilesTypes[i]);
		}
	}

	private void HideExcessTiles()
	{
		int iNewGridSize = m_iGridSizeX * m_iGridSizeY;

		for (int i = m_pTiles.Count - 1; i >= iNewGridSize; --i)
		{
			m_pTiles[i].gameObject.SetActive(false);
		}
	}

	private List<char> ExtractMapDataFromString(string sMapName)
	{
		List<char> pTilesTypes = null;

		StreamReader pStreamReader = new StreamReader(Application.streamingAssetsPath + "/Maps/" + sMapName + ".txt");

		using (pStreamReader)
		{
			string sLine = pStreamReader.ReadLine();
			if (!sLine.StartsWith("x=") || !int.TryParse(sLine.Substring(2), out m_iGridSizeX))
			{
				// Error
				return null;
			}

			sLine = pStreamReader.ReadLine();
			if (!sLine.StartsWith("y=") || !int.TryParse(sLine.Substring(2), out m_iGridSizeY))
			{
				// Error
				return null;
			}


			pTilesTypes = new List<char>(m_iGridSizeX * m_iGridSizeY);
			char[] cCurrentChar = new char[1];

			while (!pStreamReader.EndOfStream)
			{
				pStreamReader.Read(cCurrentChar, 0, 1);

				if (!char.IsControl(cCurrentChar[0]))
					pTilesTypes.Add(cCurrentChar[0]);
			}
		}

		return pTilesTypes;
	}

	private void InitTileWithType(Tile pTile, ETileType eTileType)
	{
		pTile.SetTileType(eTileType);

		switch (eTileType)
		{
			case ETileType.PELLET:
				pTile.m_pPellet.m_pPickupEffect = m_pRegularPelletsEffect;
				break;
			case ETileType.SUPER_PELLET:
				pTile.m_pPellet.m_pPickupEffect = m_pSuperPelletsEffect;
				break;
			case ETileType.WARP:
				InitPortal(pTile);
				break;
			case ETileType.PACMAN_SPAWNER:
				GameManager.Instance.SetPlayerSpawnTile(pTile);
				break;
			case ETileType.GHOST_SPAWNER:
				GameManager.Instance.AddGhostSpawnTile(pTile);
				break;
		}
	}

	private void InitPortal(Tile pTile)
	{
		if (m_pUnconnectedPortal == null)
		{
			m_pUnconnectedPortal = pTile;
		}
		else
		{
			pTile.SetConnectedPortal(m_pUnconnectedPortal);
			m_pUnconnectedPortal.SetConnectedPortal(pTile);
			m_pUnconnectedPortal = null;
		}

		m_pPortalVisuals[m_iPlacedPortalVisuals].position = pTile.transform.position;
		++m_iPlacedPortalVisuals;
	}

	public void ResetTilesWithCurrentMap()
	{
		for (int i = 0; i < m_pTiles.Count; ++i)
			m_pTiles[i].InitPellet();
	}

	public Tile GetTileFromPosition(float fPosX, float fPosZ)
	{
		Vector2 tHalfMapSize = GetHalfMapSize();

		fPosX += tHalfMapSize.x;
		fPosX /= m_fTilesSize;

		fPosZ -= tHalfMapSize.y;
		fPosZ /= m_fTilesSize;

		int iTileIndex = (int)fPosX - ((int)fPosZ * m_iGridSizeX);

		return (iTileIndex >= 0 && iTileIndex < m_pTiles.Count) ? m_pTiles[iTileIndex] : null;
	}

	public Vector3 GetPosOnMapFurtherFromPosition(Vector3 tPos)
	{
		Vector2 tHalfMapSize = GetHalfMapSize();

		float fXInNearestConer = tHalfMapSize.x * (tPos.x >= 0.0f ? 1.0f : -1.0f);
		float fZInNearestConer = tHalfMapSize.y * (tPos.z >= 0.0f ? 1.0f : -1.0f);

		Vector3 tPosPushedOnNearestCorner = new Vector3(fXInNearestConer, 0.0f, fZInNearestConer);

		return -tPosPushedOnNearestCorner;	// Opposite corner
	}


	private Vector2 GetHalfMapSize()
	{
		float fHalfMapSizeX = (m_iGridSizeX / 2.0f) - (m_fTilesSize * 0.5f);
		float fHalfMapSizeY = (m_iGridSizeY / 2.0f) - (m_fTilesSize * 0.5f);

		return new Vector2(fHalfMapSizeX, fHalfMapSizeY);
	}
}

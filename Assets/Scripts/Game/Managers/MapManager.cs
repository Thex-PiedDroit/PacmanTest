
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
		List<char> pTilesTypes;
		if (!ExtractMapDataFromString(sMapName, out pTilesTypes))
			return;

		HideExcessTiles();
		ScoresManager.Instance.ResetPelletsCountInMap();

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

			ETileType eTileType = (ETileType)pTilesTypes[i];

			if (!Tile.IsTileTypeValid(eTileType))
				DisplayParsingError("A tile in the map file " + sMapName + " has an unrecognized type (" + pTilesTypes[i] + "). Please fix this.");

			InitTileWithType(pCurrentTile, eTileType);
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

	private bool ExtractMapDataFromString(string sMapName, out List<char> pTilesTypes)
	{
		pTilesTypes = null;

		StreamReader pStreamReader = new StreamReader(Application.streamingAssetsPath + "/Maps/" + sMapName + ".txt");

		using (pStreamReader)
		{
			string sLine = pStreamReader.ReadLine();
			if (!sLine.StartsWith("x=") || !int.TryParse(sLine.Substring(2), out m_iGridSizeX))
			{
				DisplayParsingError("The map file " + sMapName + " has errors in it. The first line should be \"x=\" followed by the map size along the X axis. Please fix this.");
				return false;
			}

			sLine = pStreamReader.ReadLine();
			if (!sLine.StartsWith("y=") || !int.TryParse(sLine.Substring(2), out m_iGridSizeY))
			{
				DisplayParsingError("The map file " + sMapName + " has errors in it. The second line should be \"y=\" followed by the map size along the Y axis. Please fix this.");
				return false;
			}


			int iMapTotalSize = m_iGridSizeX * m_iGridSizeY;

			pTilesTypes = new List<char>(iMapTotalSize);
			char[] cCurrentChar = new char[1];

			while (!pStreamReader.EndOfStream)
			{
				pStreamReader.Read(cCurrentChar, 0, 1);

				if (!char.IsControl(cCurrentChar[0]))
					pTilesTypes.Add(cCurrentChar[0]);
			}


			if (pTilesTypes.Count != iMapTotalSize)
			{
				DisplayParsingError("The map file " + sMapName + " has errors in it. There are not the same amount of tiles as deduced from the map size. Please fix this.");
				pTilesTypes = null;

				return false;
			}
		}

		return true;
	}

	private void DisplayParsingError(string sErrorText)
	{
		Toolkit.DisplayImportantErrorMessage("Map file parsing error", sErrorText);
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
				GhostsManager.Instance.AddGhostSpawnTile(pTile);
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

	public Tile GetTileFromPosition(Vector3 tPos)
	{
		return GetTileFromPosition(tPos.x, tPos.z);
	}

	public Tile GetTileFromPosition(float fPosX, float fPosZ)
	{
		Vector2 tHalfMapSize = GetHalfMapSize();

		fPosX += tHalfMapSize.x;
		fPosX /= m_fTilesSize;

		fPosZ -= tHalfMapSize.y;
		fPosZ /= m_fTilesSize;

		int iTileIndex = Mathf.RoundToInt(fPosX) - (Mathf.RoundToInt(fPosZ) * m_iGridSizeX);

		return (iTileIndex >= 0 && iTileIndex < m_pTiles.Count) ? m_pTiles[iTileIndex] : null;
	}

	public Tile GetAdjacentTileFurtherFromPosition(Vector3 tPosToAvoid, Tile pCurrentTile, Tile pExcludedTile)
	{
		Vector3 tCurrentPos = pCurrentTile.transform.position;

		/*		Check directly in opposite direction fist		*/
		Vector3 tOppositeDirection = Toolkit.FlattenDirectionOnOneAxis(tCurrentPos - tPosToAvoid);
		Tile pTileFurther = GetWalkableTileInDirection(tOppositeDirection, tCurrentPos);

		if (pTileFurther != null && pTileFurther != pExcludedTile && pTileFurther.IsWalkable())
			return pTileFurther;


		/*		Then check both left and right and return the nearest one (or none if to exclude)		*/
		Vector3 tLeftDirection = tOppositeDirection.Rotate90AroundY();
		Vector3 tRightDirection = -tLeftDirection;

		Tile pLeftTile = GetWalkableTileInDirection(tLeftDirection, tCurrentPos);
		if (pLeftTile == null || pLeftTile == pExcludedTile || !pLeftTile.IsWalkable())
			pLeftTile = null;

		Tile pRightTile = GetWalkableTileInDirection(tRightDirection, tCurrentPos);
		if (pRightTile == null || pRightTile == pExcludedTile || !pRightTile.IsWalkable())
			pRightTile = null;

		/*		Check if one clear winner		*/
		if (pLeftTile == null && pRightTile == null)
			return null;
		if (pLeftTile == null && pRightTile != null)
			return pRightTile;
		if (pRightTile == null && pLeftTile != null)
			return pLeftTile;

		/*		If both possible, return the one further from target		*/
		float fSqrdDistanceToLeft = (pLeftTile.transform.position - tPosToAvoid).sqrMagnitude;
		float fSqrdDistanceToRight = (pRightTile.transform.position - tPosToAvoid).sqrMagnitude;

		return fSqrdDistanceToLeft >= fSqrdDistanceToRight ? pLeftTile : pRightTile;
	}

	public Tile GetWalkableTileInDirection(Vector3 tDirection, Vector3 tFromPosition)
	{
		const float c_fDistanceInFrontOfPositionToRaycastFrom = 1.1f;   // Slightly more than 1.0f in order to be sure to reach the next tile even from the very edge of one

		Tile pTile = GetTileFromPosition(tFromPosition + (tDirection * c_fDistanceInFrontOfPositionToRaycastFrom));
		return pTile.IsWalkable() ? pTile : null;
	}


	private Vector2 GetHalfMapSize()
	{
		float fHalfMapSizeX = (m_iGridSizeX / 2.0f) - (m_fTilesSize * 0.5f);
		float fHalfMapSizeY = (m_iGridSizeY / 2.0f) - (m_fTilesSize * 0.5f);

		return new Vector2(fHalfMapSizeX, fHalfMapSizeY);
	}
}

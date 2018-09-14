
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;


public class TilesGenerator : MonoBehaviour
{
#region Variables (public)

	public string m_sMapFileName = "Map1";
	public float m_fTilesSize = 1.0f;

	public Tile m_pTilePrefab = null;

	public List<Tile> m_pTiles = null;

	#endregion

#region Variables (private)

	private int m_iGridSizeX = 0;
	private int m_iGridSizeY = 0;

	#endregion


	public void GenerateTiles()
	{
		HideExcessTiles();

		List<char> pTilesTypes = ExtractMapDataFromString();

		for (int i = 0; i < pTilesTypes.Count; ++i)
		{
			int iX = i % m_iGridSizeX;
			int iY = i / m_iGridSizeX;

			float fPosX = (iX - (m_iGridSizeX / 2.0f)) * m_fTilesSize;
			float fPosY = (-iY + (m_iGridSizeY / 2.0f)) * m_fTilesSize;

			Tile pCurrentTile = null;

			if (m_pTiles.Count <= i)
			{
				if (Application.isPlaying)
				{
					pCurrentTile = Instantiate(m_pTiles[0], transform);
				}
				else
				{
					pCurrentTile = PrefabUtility.InstantiatePrefab(m_pTilePrefab) as Tile;
					pCurrentTile.transform.parent = transform;
				}

				m_pTiles.Add(pCurrentTile);
			}
			else
			{
				pCurrentTile = m_pTiles[i];
				pCurrentTile.gameObject.SetActive(true);
			}

			pCurrentTile.transform.localPosition = new Vector3(fPosX, 0.0f, fPosY);
			pCurrentTile.SetTileType((ETileType)pTilesTypes[i]);
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

	private List<char> ExtractMapDataFromString()
	{
		List<char> pTilesTypes = null;

		StreamReader pStreamReader = new StreamReader(Application.streamingAssetsPath + "/Maps/" + m_sMapFileName + ".txt");

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
}

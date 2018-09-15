
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
#region Variables (public)

	static public GameManager Instance = null;

	public GameModeData m_pGameModeData = null;

	public PlayerCharacter m_pPlayerCharacterPrefab = null;
	public Ghost m_pGhostsPrefab = null;

	public Transform m_pPlayerCharacterContainer = null;
	public Transform m_pGhostsContainer = null;

	public List<GhostBehaviour> m_pGhostsBehaviours = null;

	#endregion

#region Variables (private)

	private Tile m_pPlayerSpawnTile = null;
	private List<Tile> m_pGhostSpawnTiles = null;
	private List<Ghost> m_pGhosts = null;

	private int m_iCurrentPlayerLives = 0;
	private int m_iCurrentDeadGhosts = 0;

	private bool m_bAboutToReleaseAGhost = false;

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;

		m_pGhostSpawnTiles = new List<Tile>();
		m_pGhosts = new List<Ghost>();
	}

	private void Start()
	{
		MapManager.Instance.LoadMap(m_pGameModeData.m_sDefaultMapName);
		LaunchGame();
	}

	private void OnDestroy()
	{
		if (PlayerCharacter.Instance != null)
			PlayerCharacter.Instance.OnDeath -= PlayerGotKilled;

		for (int i = 0; i < m_pGhosts.Count; ++i)
		{
			if (m_pGhosts[i] != null)
				m_pGhosts[i].OnDeath -= GhostGotKilled;
		}
	}

	private void LaunchGame()
	{
		m_iCurrentPlayerLives = m_pGameModeData.m_iInitialPlayerLives;
		SpawnPlayerCharacter();
		SpawnGhosts();

			// TODO: Game start countdown here
		PlayerCharacter.Instance.MakePlayerAlive();
		ReleaseNextGhostNow();
	}

	private void SpawnPlayerCharacter()
	{
		PlayerCharacter pPlayer = PlayerCharacter.Instance ?? Instantiate(m_pPlayerCharacterPrefab, m_pPlayerCharacterContainer);
		pPlayer.transform.position = m_pPlayerSpawnTile.transform.position;

		pPlayer.OnDeath -= PlayerGotKilled;
		pPlayer.OnDeath += PlayerGotKilled;
	}

	private void RespawnPlayer()
	{
		PlayerCharacter pPlayer = PlayerCharacter.Instance;
		pPlayer.transform.position = m_pPlayerSpawnTile.transform.position;
		pPlayer.m_pPickupsModule.ResetCollectedPellets();
	}

	private void RespawnGame()
	{
		RespawnPlayer();
		SpawnGhosts();

		PlayerCharacter.Instance.MakePlayerAlive();
		ReleaseNextGhostNow();
	}

	private void GameOver()
	{
		// TODO: Implement game over
	}

	private void SpawnGhosts()
	{
		StopAllCoroutines();

		for (int i = 0; i < m_pGhostSpawnTiles.Count; ++i)
		{
			Ghost pGhost = null;

			if (i >= m_pGhosts.Count)
			{
				pGhost = Instantiate(m_pGhostsPrefab, m_pGhostsContainer);
				m_pGhosts.Add(pGhost);
			}
			else
			{
				pGhost = m_pGhosts[i];
			}

			pGhost.transform.position = m_pGhostSpawnTiles[i].transform.position;

			pGhost.OnDeath -= GhostGotKilled;
			pGhost.OnDeath += GhostGotKilled;

			pGhost.GiveBehaviour(m_pGhostsBehaviours[i % m_pGhostsBehaviours.Count]);
			pGhost.SetDead();
		}

		m_iCurrentDeadGhosts = m_pGhosts.Count;
	}

	private void ReleaseNextGhostNow()
	{
		for (int i = 0; i < m_pGhosts.Count; ++i)
		{
			if (m_pGhosts[i].IsDead())
			{
				m_pGhosts[i].SetAlive();
				--m_iCurrentDeadGhosts;
				break;
			}
		}

		if (m_iCurrentDeadGhosts > 0)
			StartCoroutine(ReleaseNextGhostAfterDelay());
	}


	private IEnumerator ReleaseNextGhostAfterDelay()
	{
		m_bAboutToReleaseAGhost = true;

		float fStartTime = Time.time;
		while (Time.time - fStartTime < m_pGameModeData.m_fSecondsBetweenGhostsActivation)
			yield return false;

		m_bAboutToReleaseAGhost = false;
		ReleaseNextGhostNow();
	}


	private void PlayerGotKilled()
	{
		--m_iCurrentPlayerLives;

		if (m_iCurrentPlayerLives == 0)
			GameOver();
		else
			RespawnGame();
	}

	private void GhostGotKilled()
	{
		++m_iCurrentPlayerLives;

		if (!m_bAboutToReleaseAGhost)
			StartCoroutine(ReleaseNextGhostAfterDelay());
	}

	public void SetPlayerSpawnTile(Tile pTile)
	{
		m_pPlayerSpawnTile = pTile;
	}

	public void AddGhostSpawnTile(Tile pTile)
	{
		m_pGhostSpawnTiles.Add(pTile);
	}
}

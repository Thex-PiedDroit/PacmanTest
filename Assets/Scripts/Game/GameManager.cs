
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

	public FleeGhostBehaviour m_pFleeGhostBehaviour = null;
	public List<GhostBehaviour> m_pGhostsBehaviours = null;

	public bool SuperPelletEffectAboutToWearOut { get; set; } = false;

	#endregion

#region Variables (private)

	private const string c_sVariableName_pBehaviourGhostHadBeforeFleeing = "pBehaviourGhostHadBeforeFleeing";

	private Tile m_pPlayerSpawnTile = null;
	private List<Tile> m_pGhostSpawnTiles = null;
	private List<Ghost> m_pGhosts = null;

	private int m_iCurrentPlayerLives = 0;
	private int m_iCurrentDeadGhosts = 0;

	private int m_iNextGhostToReleaseIfPossible = 0;
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

	public void RestartGame()
	{
		PlayerCharacter.Instance.m_pPickupsModule.ResetScores();

		EndGameScreenManager.Instance.HideEndGameScreen();
		ScoresManager.Instance.SetDisplayVisible(true);

		MapManager.Instance.ResetTilesWithCurrentMap();
		LaunchGame();
	}

	private void LaunchGame()
	{
		m_iCurrentPlayerLives = m_pGameModeData.m_iInitialPlayerLives;
		ScoresManager.Instance.InitLives(m_iCurrentPlayerLives);

		SpawnPlayerCharacter();
		SpawnGhosts();

			// TODO: Game start countdown here
		PlayerCharacter.Instance.MakePlayerAlive();
		ReleaseNextGhostNow();
	}

	public void QuitGame()
	{
		Application.Quit();	// TODO: Add "Are you sure?" dialog
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
		SetAllGhostsDead();
		EndGameScreenManager.Instance.DisplayEndGameScreen();
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

			pGhost.SetSpawnTile(m_pGhostSpawnTiles[i]);
			pGhost.m_pProceduralVariablesModule.ResetAllVariables();
			pGhost.GiveBehaviour(m_pGhostsBehaviours[i % m_pGhostsBehaviours.Count]);
			pGhost.SetDead();
		}

		m_iCurrentDeadGhosts = m_pGhosts.Count;
	}

	private void ReleaseNextGhostNow()
	{
		int iGhostsCount = m_pGhosts.Count;

		for (int i = 0; i < iGhostsCount; ++i)
		{
			int iGhostIndex = (i + m_iNextGhostToReleaseIfPossible) % iGhostsCount;

			if (m_pGhosts[iGhostIndex].IsDead())
			{
				m_pGhosts[iGhostIndex].SetAlive();
				--m_iCurrentDeadGhosts;

				m_iNextGhostToReleaseIfPossible = iGhostIndex + 1;

				break;
			}
		}

		if (m_iCurrentDeadGhosts > 0)
			StartCoroutine(ReleaseNextGhostAfterDelay());
	}

	private void SetAllGhostsDead()
	{
		for (int i = 0; i < m_pGhosts.Count; ++i)
			m_pGhosts[i].SetDead();
	}

	public void GiveFleeBehaviourToGhosts()
	{
		for (int i = 0; i < m_pGhosts.Count; ++i)
		{
			if (m_pGhosts[i].m_pBehaviour != m_pFleeGhostBehaviour)
				m_pGhosts[i].m_pProceduralVariablesModule.SetVariable(c_sVariableName_pBehaviourGhostHadBeforeFleeing, m_pGhosts[i].m_pBehaviour);
			m_pGhosts[i].GiveBehaviour(m_pFleeGhostBehaviour);
		}
	}

	public void ResetGhostsToNormalBehaviour()
	{
		for (int i = 0; i < m_pGhosts.Count; ++i)
			ResetThisGhostToNormalBehaviour(m_pGhosts[i]);
	}

	private void ResetThisGhostToNormalBehaviour(Ghost pGhost)
	{
		GhostBehaviour pPreviousBehaviour = (GhostBehaviour)(pGhost.m_pProceduralVariablesModule.GetVariable(c_sVariableName_pBehaviourGhostHadBeforeFleeing));
		if (pPreviousBehaviour == null)
			return;

		pGhost.GiveBehaviour(pPreviousBehaviour);
		pGhost.m_pProceduralVariablesModule.ResetVariable(c_sVariableName_pBehaviourGhostHadBeforeFleeing);
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

	private void GhostGotKilled(Ghost pGhost)
	{
		++m_iCurrentDeadGhosts;

		if (pGhost.m_pBehaviour == m_pFleeGhostBehaviour)
			ResetThisGhostToNormalBehaviour(pGhost);

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

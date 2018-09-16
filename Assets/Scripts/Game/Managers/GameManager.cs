
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
#region Variables (public)

	static public GameManager Instance = null;


	public GameModeData m_pGameModeData = null;

	public PlayerCharacter m_pPlayerCharacterPrefab = null;

	public Transform m_pPlayerCharacterContainer = null;

	public bool SuperPelletEffectAboutToWearOut { get; set; } = false;

	#endregion

#region Variables (private)

	private Tile m_pPlayerSpawnTile = null;

	private int m_iCurrentPlayerLives = 0;

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;
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
		GhostsManager.Instance.SpawnGhosts();

			// TODO: Game start countdown here
		PlayerCharacter.Instance.MakePlayerAlive();
		GhostsManager.Instance.ReleaseNextGhostNow();
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
		GhostsManager.Instance.SpawnGhosts();

		PlayerCharacter.Instance.MakePlayerAlive();
		GhostsManager.Instance.ReleaseNextGhostNow();
	}

	private void GameOver()
	{
		GhostsManager.Instance.SetAllGhostsDead();
		EndGameScreenManager.Instance.DisplayEndGameScreen();
	}


	private void PlayerGotKilled()
	{
		--m_iCurrentPlayerLives;

		if (m_iCurrentPlayerLives == 0)
			GameOver();
		else
			RespawnGame();
	}

	public void SetPlayerSpawnTile(Tile pTile)
	{
		m_pPlayerSpawnTile = pTile;
	}
}

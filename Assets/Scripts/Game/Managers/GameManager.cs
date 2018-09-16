
using System;
using UnityEngine;


public class GameManager : MonoBehaviour
{
#region Variables (public)

	static public GameManager Instance = null;


	/// <summary>
	/// Both from GameOver and Win
	/// </summary>
	public Action OnGameEnd = null;

	public GameModeData m_pGameModeData = null;

	public PlayerCharacter m_pPlayerCharacterPrefab = null;

	public Transform m_pPlayerCharacterContainer = null;

	public CountDownModule m_pCountDownModule = null;

	public bool SuperPelletEffectAboutToWearOut { get; set; } = false;

	#endregion

#region Variables (private)

	private Tile m_pPlayerSpawnTile = null;

	private int m_iCurrentPlayerLives = 0;

	private bool m_bGameIsPlaying = false;

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

		if (m_pCountDownModule != null)
			m_pCountDownModule.OnCountDownFinished -= StartGameAfterCountDown;
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


		m_pCountDownModule.LaunchCountDown(m_pGameModeData.m_iBeforeStartCountDownDurationInSeconds);
		m_pCountDownModule.OnCountDownFinished += StartGameAfterCountDown;
	}

	private void StartGameAfterCountDown()
	{
		m_pCountDownModule.OnCountDownFinished -= StartGameAfterCountDown;

		PlayerCharacter.Instance.MakePlayerAlive();
		GhostsManager.Instance.ReleaseNextGhostNow();

		m_bGameIsPlaying = true;
	}

	public void QuitGame()
	{
		Application.Quit();	// TODO: Add "Are you sure?" dialog
	}

	public bool IsGamePlaying()
	{
		return m_bGameIsPlaying;
	}

	private void SpawnPlayerCharacter()
	{
		PlayerCharacter pPlayer = PlayerCharacter.Instance ?? Instantiate(m_pPlayerCharacterPrefab, m_pPlayerCharacterContainer);
		pPlayer.SetSpawnTile(m_pPlayerSpawnTile);
		pPlayer.TeleportToSpawn();

		pPlayer.OnDeath -= PlayerGotKilled;
		pPlayer.OnDeath += PlayerGotKilled;
	}

	private void RespawnPlayer()
	{
		PlayerCharacter pPlayer = PlayerCharacter.Instance;
		pPlayer.TeleportToSpawn();
		pPlayer.m_pPickupsModule.ResetCollectedPellets();
	}

	private void RespawnGame()
	{
		SetGameStopped();

		RespawnPlayer();
		GhostsManager.Instance.SpawnGhosts();

		m_pCountDownModule.LaunchCountDown(m_pGameModeData.m_iBeforeStartCountDownDurationInSeconds);
		m_pCountDownModule.OnCountDownFinished += StartGameAfterCountDown;
	}

	private void SetGameStopped()
	{
		m_bGameIsPlaying = false;
		OnGameEnd?.Invoke();
	}

	private void GameOver()
	{
		SetGameStopped();

		GhostsManager.Instance.SetAllGhostsDead();
		EndGameScreenManager.Instance.DisplayEndGameScreen();
	}

	public void WinGame()
	{
		SetGameStopped();

		PlayerCharacter.Instance.ResetPlayer();
		MapManager.Instance.ResetTilesWithCurrentMap();
		LaunchGame();
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


using TMPro;
using UnityEngine;


public class EndGameScreenManager : MonoBehaviour
{
#region Variables (public)

	static public EndGameScreenManager Instance = null;


	public GameObject m_pEndGameScreen = null;

	public TextMeshProUGUI m_pScoreValueText = null;
	public TextMeshProUGUI m_pHighScoreValueText = null;

	public GameObject m_pNotHighScoreDisplay = null;
	public GameObject m_pNewHighScoreDisplay = null;

	#endregion

#region Variables (private)

	private const string c_sSavedVariableName_iHighScore = "iHighScore";

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;
	}

	public void DisplayEndGameScreen()
	{
		ScoresManager.Instance.SetDisplayVisible(false);
		ScoresManager.Instance.ResetCollectedPellets();

		m_pScoreValueText.text = ScoresManager.Instance.m_pScoreValueText.text;
		RegisterAndDisplayHighScore();

		m_pEndGameScreen.SetActive(true);
	}

	public void HideEndGameScreen()
	{
		m_pEndGameScreen.SetActive(false);
	}

	private void RegisterAndDisplayHighScore()
	{
		int iScore = int.Parse(m_pScoreValueText.text);
		int iHighScore = PlayerPrefs.GetInt(c_sSavedVariableName_iHighScore, 0);

		m_pNewHighScoreDisplay.SetActive(false);
		m_pNotHighScoreDisplay.SetActive(false);

		if (iScore > iHighScore)
		{
			iHighScore = iScore;
			PlayerPrefs.SetInt(c_sSavedVariableName_iHighScore, iHighScore);

			m_pNewHighScoreDisplay.SetActive(true);
		}
		else
		{
			m_pHighScoreValueText.text = iHighScore.ToString();
			m_pNotHighScoreDisplay.SetActive(true);
		}
	}
}


using TMPro;
using UnityEngine;


public class ScoresManager : MonoBehaviour
{
#region Variables (public)

	public PlayerCharacter m_pPlayerCharacter = null;

	public TextMeshProUGUI m_pScoreValueText = null;

	#endregion

#region Variables (private)



	#endregion


	private void Awake()
	{
		m_pPlayerCharacter.m_pPickupsModule.OnPointsGained += UpdateScore;
	}

	private void OnDestroy()
	{
		if (m_pPlayerCharacter != null && m_pPlayerCharacter.m_pPickupsModule != null)
			m_pPlayerCharacter.m_pPickupsModule.OnPointsGained -= UpdateScore;
	}

	private void UpdateScore(int iCurrentScore)
	{
		m_pScoreValueText.text = iCurrentScore.ToString();
	}
}

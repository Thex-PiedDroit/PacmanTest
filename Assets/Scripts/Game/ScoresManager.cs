
using TMPro;
using UnityEngine;


public class ScoresManager : MonoBehaviour
{
#region Variables (public)

	public TextMeshProUGUI m_pScoreValueText = null;

	#endregion

#region Variables (private)



	#endregion


	private void Start()
	{
		PlayerCharacter.Instance.m_pPickupsModule.OnPointsGained += UpdateScore;
	}

	private void OnDestroy()
	{
		PlayerCharacter pPlayer = PlayerCharacter.Instance;

		if (pPlayer != null && pPlayer.m_pPickupsModule != null)
			pPlayer.m_pPickupsModule.OnPointsGained -= UpdateScore;
	}

	private void UpdateScore(int iCurrentScore)
	{
		m_pScoreValueText.text = iCurrentScore.ToString();
	}
}

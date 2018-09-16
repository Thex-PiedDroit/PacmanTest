
using TMPro;
using UnityEngine;
using System.Collections.Generic;


public class ScoresManager : MonoBehaviour
{
#region Variables (public)

	static public ScoresManager Instance = null;


	public GameObject m_pScoresDisplay = null;

	public TextMeshProUGUI m_pScoreValueText = null;

	public int m_iLivesSpritesPerRow = 5;
	public List<RectTransform> m_pLivesSprites = null;

	#endregion

#region Variables (private)

	private int m_iCurrentLivesDisplayed = 0;

	#endregion


	private void Awake()
	{
		if (!Toolkit.InitSingleton(this, ref Instance))
			return;
	}

	private void Start()
	{
		PlayerCharacter.Instance.m_pPickupsModule.OnPointsGained += UpdateScore;
		PlayerCharacter.Instance.OnDeath += RemoveLife;
	}

	private void OnDestroy()
	{
		PlayerCharacter pPlayer = PlayerCharacter.Instance;

		if (pPlayer != null)
		{
			if (pPlayer.m_pPickupsModule != null)
				pPlayer.m_pPickupsModule.OnPointsGained -= UpdateScore;

			pPlayer.OnDeath += RemoveLife;
		}
	}

	private void UpdateScore(int iCurrentScore)
	{
		m_pScoreValueText.text = iCurrentScore.ToString();
	}

	public void InitLives(int iLivesCount)
	{
		int iHigherBetweenLivesCountAndExistingSprite = iLivesCount >= m_pLivesSprites.Count ? iLivesCount : m_pLivesSprites.Count;

		for (int i = 0; i < iHigherBetweenLivesCountAndExistingSprite; ++i)
		{
			RectTransform pCurrentLifeSprite = i < m_pLivesSprites.Count ? m_pLivesSprites[i] : CreateNewLifeSprite();
			pCurrentLifeSprite.gameObject.SetActive(i < iLivesCount);
		}

		m_iCurrentLivesDisplayed = iLivesCount;
	}

	private RectTransform CreateNewLifeSprite()
	{
		int iCurrentSpritesCount = m_pLivesSprites.Count;
		RectTransform pPreviousSprite = m_pLivesSprites[iCurrentSpritesCount - 1];

		RectTransform pNewSprite = Instantiate(pPreviousSprite, pPreviousSprite.parent);

		float fX = (iCurrentSpritesCount % m_iLivesSpritesPerRow) * pPreviousSprite.rect.width;
		float fY = -(iCurrentSpritesCount / m_iLivesSpritesPerRow) * pPreviousSprite.rect.height;

		pNewSprite.anchoredPosition = new Vector2(fX, fY);
		m_pLivesSprites.Add(pNewSprite);

		return pNewSprite;
	}

	private void RemoveLife()
	{
		if (m_iCurrentLivesDisplayed == 0)
			return;

		--m_iCurrentLivesDisplayed;
		m_pLivesSprites[m_iCurrentLivesDisplayed].gameObject.SetActive(false);
	}

	public void SetDisplayVisible(bool bVisible)
	{
		m_pScoresDisplay.SetActive(bVisible);
	}
}

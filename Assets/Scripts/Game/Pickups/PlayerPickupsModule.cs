
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;


public class PlayerPickupsModule : CharacterProceduralVariablesModule
{
#region Variables (public)

	/// <summary>
	/// int is iCurrentPoints;
	/// </summary>
	public Action<int> OnPointsGained = null;

	#endregion

#region Variables (private)

	private int m_iCollectedPellets = 0;
	private int m_iGamePoints = 0;
	private List<PickupEffect> m_pCurrentActivePickupEffects = null;

	#endregion


	override protected void Awake()
	{
		base.Awake();
		m_pCurrentActivePickupEffects = new List<PickupEffect>();
	}

	private void Update()
	{
		for (int i = m_pCurrentActivePickupEffects.Count - 1; i >= 0; --i)	// In reverse cause effects might remove themselves while looping
			m_pCurrentActivePickupEffects[i].UpdateEffect(this);
	}

	public void GivePoints(int iPoints)
	{
		m_iGamePoints += iPoints;
		OnPointsGained?.Invoke(m_iGamePoints);
	}

	public void RegisterPelletCollect()
	{
		++m_iCollectedPellets;
	}

	public int GetCurrentPelletsCount()
	{
		return m_iCollectedPellets;
	}

	public void ResetCollectedPellets()
	{
		m_iCollectedPellets = 0;
	}

	public void ResetScores()
	{
		m_iGamePoints = 0;
		OnPointsGained?.Invoke(m_iGamePoints);

		m_iCollectedPellets = 0;
	}

	public void GiveActiveEffect(PickupEffect pEffect)
	{
		Assert.IsFalse(m_pCurrentActivePickupEffects.Contains(pEffect), "An effect (" + pEffect.name + ") has been added to the effects list but was already in it. This is not supported behaviour, please fix or change the code to allow it");
		m_pCurrentActivePickupEffects.Add(pEffect);
	}

	public bool HasActiveEffect(PickupEffect pEffect)
	{
		return m_pCurrentActivePickupEffects.Contains(pEffect);
	}

	public void RemoveActiveEffect(PickupEffect pEffect)
	{
		m_pCurrentActivePickupEffects.Remove(pEffect);
	}
}

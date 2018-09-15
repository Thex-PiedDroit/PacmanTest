
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;


public class PlayerPickupsModule : MonoBehaviour
{
#region Variables (public)

	/// <summary>
	/// int is iCurrentPoints;
	/// </summary>
	public Action<int> OnPointsGained = null;

	#endregion

#region Variables (private)

	private int m_iGamePoints = 0;

	/// <summary>
	/// string is sVariableName;
	/// object is pVariableValue;
	/// </summary>
	private Dictionary<string, object> m_pProceduralVariables = null;

	private List<PickupEffect> m_pCurrentActivePickupEffects = null;

	#endregion


	private void Awake()
	{
		m_pProceduralVariables = new Dictionary<string, object>();
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

	public void ResetPoints()
	{
		m_iGamePoints = 0;
		OnPointsGained?.Invoke(m_iGamePoints);
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

	public object GetVariable(string sVariableName)
	{
		return m_pProceduralVariables.ContainsKey(sVariableName) ? m_pProceduralVariables[sVariableName] : null;
	}

	public bool GetVariableAsBool(string sVariableName)
	{
		object pValue = GetVariable(sVariableName);
		return pValue != null ? (bool)pValue : false;
	}

	public void SetVariable(string sVariableName, object pValue)
	{
		m_pProceduralVariables[sVariableName] = pValue;
	}

	public void ResetVariable(string sVariableName)
	{
		if (m_pProceduralVariables.ContainsKey(sVariableName))
			m_pProceduralVariables[sVariableName] = null;
	}

	private void ResetVariables()
	{
		m_pProceduralVariables.Clear();
	}
}

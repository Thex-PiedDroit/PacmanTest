
using UnityEngine;
using System.Collections.Generic;


public class CharacterProceduralVariablesModule : MonoBehaviour
{
#region Variables (public)



	#endregion

#region Variables (private)

	/// <summary>
	/// string is sVariableName;
	/// object is pVariableValue;
	/// </summary>
	private Dictionary<string, object> m_pProceduralVariables = null;

	#endregion


	virtual protected void Awake()
	{
		m_pProceduralVariables = new Dictionary<string, object>();
	}

	public object GetVariable(string sVariableName, object pDefaultValue = null)
	{
		return m_pProceduralVariables.ContainsKey(sVariableName) ? m_pProceduralVariables[sVariableName] : pDefaultValue;
	}

	public bool GetVariableAsBool(string sVariableName)
	{
		return (bool)GetVariable(sVariableName, false);
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

	public void ResetAllVariables()
	{
		m_pProceduralVariables.Clear();
	}
}

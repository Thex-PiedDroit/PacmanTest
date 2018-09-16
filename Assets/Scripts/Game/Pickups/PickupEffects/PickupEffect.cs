
using UnityEngine;


abstract public class PickupEffect : ScriptableObject
{
#region Variables (public)

	public int m_iPointsWorth = 0;

	#endregion

#region Variables (private)



	#endregion


	public void GivePointsToPlayer(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.GivePoints(m_iPointsWorth);
	}

	abstract public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule);

	/// <summary>
	/// Empty by default
	/// </summary>
	virtual public void UpdateEffect(PlayerPickupsModule pPickupsModule)
	{

	}

	/// <summary>
	/// Has to be called by children!
	/// </summary>
	virtual public void EffectEnd(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.RemoveActiveEffect(this);
	}
}

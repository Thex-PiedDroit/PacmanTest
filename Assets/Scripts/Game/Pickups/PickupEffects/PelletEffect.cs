
using UnityEngine;


[CreateAssetMenu(fileName = "PelletEffect", menuName = "PickupEffects/RegularPellet")]
public class PelletEffect : PickupEffect
{
#region Variables (public)



	#endregion

#region Variables (private)



	#endregion


	override public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.RegisterPelletCollect();
		// Doesn't do anything else, shouldn't give itself to the module for update or anything
	}
}

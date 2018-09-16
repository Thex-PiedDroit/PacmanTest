
using UnityEngine;
using UnityEngine.Assertions;


public class Pickup : MonoBehaviour
{
#region Variables (public)

	public PickupEffect m_pPickupEffect = null;

	#endregion

	#region Variables (private)



	#endregion


	private void OnTriggerEnter(Collider pOther)
	{
		if (m_pPickupEffect == null)
			return;

		PlayerCharacter pPlayer = pOther.GetComponent<PlayerCharacter>();
		Assert.IsTrue(pPlayer != null, "Something that is not a player character has triggered a pickup. Please make sure the layers and collisions are correctly setup. Object in question is " + pOther.name + " and pickup is " + name);

		gameObject.SetActive(false);
		m_pPickupEffect.GivePointsToPlayer(pPlayer.m_pPickupsModule);
		m_pPickupEffect.GiveEffectToPlayer(pPlayer.m_pPickupsModule);
	}
}

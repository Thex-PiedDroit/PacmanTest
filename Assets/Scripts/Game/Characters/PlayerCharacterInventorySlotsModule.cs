
using UnityEngine;


public enum EInventorySlot
{
	HAND,
	BELT,
	BACK,
	NONE
}

public class PlayerCharacterInventorySlotsModule : MonoBehaviour
{
#region Variables (public)

	public Transform m_pHandSlot = null;
	public Transform m_pBeltSlot = null;
	public Transform m_pBackSlot = null;

	#endregion

#region Variables (private)



	#endregion


	public void EquipItemInSlot(Transform pItem, EInventorySlot eSlot, bool bKeepWorldTransform)
	{
		switch (eSlot)
		{
			case EInventorySlot.HAND:
				pItem.SetParent(m_pHandSlot, bKeepWorldTransform);
				break;
			case EInventorySlot.BELT:
				pItem.SetParent(m_pBeltSlot, bKeepWorldTransform);
				break;
			case EInventorySlot.BACK:
				pItem.SetParent(m_pBackSlot, bKeepWorldTransform);
				break;
		}
	}
}

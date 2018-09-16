
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "GrapplingHookEffect", menuName = "PickupEffects/GrapplingHook")]
public class GrapplingHookEffect : PickupEffect
{
#region Variables (public)

	public string m_sShootInputName = "UseGrapplingHook";

	public GrapplingHook m_pHookPrefab = null;

	public float m_fAnticipationDurationInSeconds = 0.5f;
	public float m_fAnticipationHandFinalDistanceBehind = 0.5f;

	public float m_fTimeBetweenHookingAndTractionInSeconds = 0.1f;

	public float m_fTractionSpeed = 40.0f;

	#endregion

#region Variables (private)

	private const string c_sVariableName_fGrapplingHookUseTime = "fGrapplingHookUseTime";

	private const string c_sVariableName_tGrapplingHookTractionDestination = "tGrapplingHookTractionDestination";
	private const string c_sVariableName_pGrapplingHookObject = "pGrapplingHookObject";

	private const string c_sVariableName_fGrapplingHookHookingTime = "fGrapplingHookHookingTime";
	private const string c_sVariableName_bGrapplingHookHitWall = "bGrapplingHookHitWall";

	private const string c_sVariableName_bGrapplingHookAborted = "bGrapplingHookAborted";


	private const float c_fMaxRaycastDistance = 20.0f;

	#endregion


	override public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.GiveActiveEffect(this);

		ResetVariables(pPickupsModule);

		GrapplingHook pHook = Instantiate(m_pHookPrefab);
		pPickupsModule.m_pMaster.m_pInventorySlotsModule.EquipItemInSlot(pHook.transform, EInventorySlot.BELT, false);
		pPickupsModule.SetVariable(c_sVariableName_pGrapplingHookObject, pHook);
	}

	private void ResetVariables(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.ResetVariable(c_sVariableName_fGrapplingHookUseTime);
		pPickupsModule.ResetVariable(c_sVariableName_tGrapplingHookTractionDestination);
		pPickupsModule.ResetVariable(c_sVariableName_pGrapplingHookObject);
		pPickupsModule.ResetVariable(c_sVariableName_fGrapplingHookHookingTime);
		pPickupsModule.ResetVariable(c_sVariableName_bGrapplingHookHitWall);
		pPickupsModule.ResetVariable(c_sVariableName_bGrapplingHookAborted);
	}

	override public void UpdateEffect(PlayerPickupsModule pPickupsModule)
	{
		if (pPickupsModule.GetVariableAsBool(c_sVariableName_bGrapplingHookAborted))
			ResetVariablesAfterAbort(pPickupsModule);

		float fUseTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fGrapplingHookUseTime, 0.0f));

		if (fUseTime != 0.0f)
		{
			UpdateShot(pPickupsModule, fUseTime);
		}
		else if (Input.GetButtonDown(m_sShootInputName))
		{
			pPickupsModule.SetVariable(c_sVariableName_fGrapplingHookUseTime, Time.time);
			pPickupsModule.m_pMaster.SetBehaviourFrozen(true);

			GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
			pPickupsModule.m_pMaster.m_pInventorySlotsModule.EquipItemInSlot(pHook.transform, EInventorySlot.HAND, false);
		}
	}

	private void UpdateShot(PlayerPickupsModule pPickupsModule, float fUseTime)
	{
		float fHookingTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fGrapplingHookHookingTime, 0.0f));

		if (fHookingTime != 0.0f)
			UpdateTraction(pPickupsModule, fHookingTime);
		else
			UpdateAnticipation(pPickupsModule, fUseTime);
	}

	private void UpdateAnticipation(PlayerPickupsModule pPickupsModule, float fUseTime)
	{
		if (Time.time - fUseTime < m_fAnticipationDurationInSeconds)
			return;

		Vector3 tShootDirection = Toolkit.QueryMoveDirectionInput();
		if (tShootDirection != Vector3.zero)
			pPickupsModule.m_pMaster.transform.forward = tShootDirection;
		else
			tShootDirection = pPickupsModule.m_pMaster.transform.forward;


		Vector3 tHookDestination = ShootHookForward(pPickupsModule, tShootDirection);

		if (tHookDestination != Vector3.down)
			InitTraction(pPickupsModule, tHookDestination);
		else
			AbortShot(pPickupsModule);
	}

	private Vector3 ShootHookForward(PlayerPickupsModule pPickupsModule, Vector3 tDirection)
	{
		RaycastHit tHit;
		if (!Physics.Raycast(pPickupsModule.m_pMaster.transform.position + (Vector3.up * 0.5f), tDirection, out tHit, c_fMaxRaycastDistance, LayerMask.GetMask("Wall", "Ghost"), QueryTriggerInteraction.Collide))
			return Vector3.down;

		Vector3 tHitPos = tHit.point - (Vector3.up * 0.5f);
		PinHookToDestination(pPickupsModule.m_pMaster, tHitPos);

		if (tHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			tHitPos = MapManager.Instance.GetTileFromPosition(tHitPos - (tDirection * 0.1f)).transform.position;
		}
		else
		{
			Ghost pHitGhost = tHit.collider.GetComponent<Ghost>();
			Assert.IsTrue(pHitGhost != null, "The grappling hook hit something (" + tHit.collider.name + ") that is not a wall but not a ghost either? Something's wrong!");

			pHitGhost.SetBehaviourFrozen(true);
		}

		return tHitPos;
	}

	private void PinHookToDestination(PlayerCharacter pPlayer, Vector3 tHookDestination)
	{
		GrapplingHook pHook = (GrapplingHook)(pPlayer.m_pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));

		pHook.m_pRopeObject.SetActive(true);

		pHook.transform.parent = null;
		pPlayer.m_pInventorySlotsModule.EquipItemInSlot(pHook.m_pRopeEnd, EInventorySlot.HAND, false);
		pHook.m_pRopeEnd.localPosition = Vector3.zero;

		pHook.transform.position = tHookDestination;
		pHook.transform.rotation = pPlayer.transform.rotation;
	}

	private void AbortShot(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.SetVariable(c_sVariableName_bGrapplingHookAborted, true);

		PlayerCharacter pPlayer = pPickupsModule.m_pMaster;
		PinHookToDestination(pPlayer, pPlayer.transform.position + (pPlayer.transform.forward * c_fMaxRaycastDistance));	// Just show the hook for a frame, for visual feedback on why it's got aborted
	}

	private void ResetVariablesAfterAbort(PlayerPickupsModule pPickupsModule)
	{
		GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
		pHook.m_pRopeEnd.SetParent(pHook.m_pRopeObject.transform);
		pHook.m_pRopeEnd.localPosition = Vector3.zero;
		pHook.m_pRopeObject.SetActive(false);

		pPickupsModule.m_pMaster.m_pInventorySlotsModule.EquipItemInSlot(pHook.transform, EInventorySlot.BELT, false);
		pHook.transform.localPosition = m_pHookPrefab.transform.localPosition;
		pHook.transform.localRotation = m_pHookPrefab.transform.localRotation;

		ResetVariables(pPickupsModule);
		pPickupsModule.SetVariable(c_sVariableName_pGrapplingHookObject, pHook);

		pPickupsModule.m_pMaster.SetBehaviourFrozen(false);
	}

	private void InitTraction(PlayerPickupsModule pPickupsModule, Vector3 tHookDestination)
	{
		PlayerCharacter pPlayer = pPickupsModule.m_pMaster;

		pPlayer.SetCurrentTileTarget(null);
		pPlayer.ClearInputsTileTarget();
		pPlayer.CanKillGhosts = true;

		pPickupsModule.SetVariable(c_sVariableName_tGrapplingHookTractionDestination, tHookDestination);
		pPickupsModule.SetVariable(c_sVariableName_fGrapplingHookHookingTime, Time.time);
	}

	private void UpdateTraction(PlayerPickupsModule pPickupsModule, float fHookingTime)
	{
		if (Time.time - fHookingTime < m_fTimeBetweenHookingAndTractionInSeconds)
			return;		// TODO: Add recoil here before traction

		Transform pPlayerTransform = pPickupsModule.m_pMaster.transform;

		Vector3 tMoveThisFrame = pPlayerTransform.forward * (m_fTractionSpeed * Time.deltaTime);

		Vector3 tHookHeadPos = (Vector3)(pPickupsModule.GetVariable(c_sVariableName_tGrapplingHookTractionDestination));
		if (pPickupsModule.GetVariableAsBool(c_sVariableName_bGrapplingHookHitWall))
			tHookHeadPos -= pPlayerTransform.forward * 0.1f;

		Vector3 tPlayerToHook = pPlayerTransform.position - tHookHeadPos;

		if (tPlayerToHook.sqrMagnitude <= tMoveThisFrame.sqrMagnitude)
		{
			pPlayerTransform.position = tHookHeadPos;

			GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
			Destroy(pHook.gameObject);

			EffectEnd(pPickupsModule);
		}
		else
		{
			pPlayerTransform.position += tMoveThisFrame;
		}
	}

	override protected void EffectEnd(PlayerPickupsModule pPickupsModule)
	{
		ResetVariables(pPickupsModule);

		pPickupsModule.RemoveActiveEffect(this);
		pPickupsModule.m_pMaster.SetBehaviourFrozen(false);
		pPickupsModule.m_pMaster.CanKillGhosts = false;
	}
}

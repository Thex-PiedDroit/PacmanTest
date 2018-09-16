
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "GrapplingHookEffect", menuName = "PickupEffects/GrapplingHook")]
public class GrapplingHookEffect : PickupEffect
{
#region Variables (public)

	public SuperGhostKillEffect m_pSuperGhostKillEffect = null;

	public string m_sShootInputName = "UseGrapplingHook";

	public GrapplingHook m_pHookPrefab = null;

	public float m_fMaxShootDistance = 50.0f;

	public float m_fAnticipationDurationInSeconds = 0.5f;
	public float m_fAnticipationHandFinalDistanceBehind = 0.5f;

	public float m_fTimeBetweenHookingAndTractionInSeconds = 0.1f;

	public float m_fTractionSpeed = 40.0f;

	#endregion

#region Variables (private)

	private enum EGrapplingHookEffectStep
	{
		AWAITING_INPUT,
		ANTICIPATION,
		TRACTION,
		ABORTED
	}

	private const string c_sVariableName_eGrapplingHookCurrentStep = "eGrapplingHookCurrentStep";
	private const string c_sVariableName_fGrapplingHookCurrentStepStartTime = "fGrapplingHookCurrentStepStartTime";

	private const string c_sVariableName_pGrapplingHookObject = "pGrapplingHookObject";
	private const string c_sVariableName_tGrapplingHookTractionDestination = "tGrapplingHookTractionDestination";

	#endregion


#region EffectInitialization

	override public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule)
	{
		if (pPickupsModule.HasActiveEffect(this))
			return;

		pPickupsModule.GiveActiveEffect(this);

		ResetVariables(pPickupsModule);
		InitHookObject(pPickupsModule);

		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.AWAITING_INPUT);
	}

	private void InitHookObject(PlayerPickupsModule pPickupsModule)
	{
		GrapplingHook pHook = Instantiate(m_pHookPrefab);
		pPickupsModule.m_pMaster.m_pInventorySlotsModule.EquipItemInSlot(pHook.transform, EInventorySlot.BELT, false);

		pPickupsModule.SetVariable(c_sVariableName_pGrapplingHookObject, pHook);
	}

	private void ResetVariables(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.ResetVariable(c_sVariableName_eGrapplingHookCurrentStep);
		pPickupsModule.ResetVariable(c_sVariableName_fGrapplingHookCurrentStepStartTime);
		pPickupsModule.ResetVariable(c_sVariableName_pGrapplingHookObject);
		pPickupsModule.ResetVariable(c_sVariableName_tGrapplingHookTractionDestination);
	}

	#endregion


	override public void UpdateEffect(PlayerPickupsModule pPickupsModule)
	{
		EGrapplingHookEffectStep eCurrentStep = (EGrapplingHookEffectStep)(pPickupsModule.GetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.AWAITING_INPUT));

		switch (eCurrentStep)
		{
			case EGrapplingHookEffectStep.AWAITING_INPUT:
				{
					if (Input.GetButtonDown(m_sShootInputName))
						InitAnticipation(pPickupsModule);
				}
				break;
			case EGrapplingHookEffectStep.ANTICIPATION:
				{
					UpdateAnticipation(pPickupsModule);
				}
				break;
			case EGrapplingHookEffectStep.TRACTION:
				{
					UpdateTraction(pPickupsModule);
				}
				break;
			case EGrapplingHookEffectStep.ABORTED:
				{
					ResetVariablesAfterAbort(pPickupsModule);
				}
				break;
		}
	}


#region Anticipation

	private void InitAnticipation(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.SetVariable(c_sVariableName_fGrapplingHookCurrentStepStartTime, Time.time);
		pPickupsModule.m_pMaster.SetBehaviourFrozen(true);

		GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
		pPickupsModule.m_pMaster.m_pInventorySlotsModule.EquipItemInSlot(pHook.transform, EInventorySlot.HAND, false);

		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.ANTICIPATION);
	}

	private void UpdateAnticipation(PlayerPickupsModule pPickupsModule)
	{
		Vector3 tShootDirection = Toolkit.QueryMoveDirectionInput();

		if (tShootDirection != Vector3.zero)
			pPickupsModule.m_pMaster.transform.forward = Toolkit.FlattenDirectionOnOneAxis(tShootDirection);
		else
			tShootDirection = pPickupsModule.m_pMaster.transform.forward;

		float fInputPressTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fGrapplingHookCurrentStepStartTime, 0.0f));
		if (Time.time - fInputPressTime < m_fAnticipationDurationInSeconds)
			return;     // TODO: Add hand animation


		Vector3 tHookDestination;
		bool bSuccess = ShootHookInDirection(pPickupsModule, tShootDirection, out tHookDestination);

		if (bSuccess)
			InitTraction(pPickupsModule, tHookDestination);
		else
			AbortShot(pPickupsModule, tHookDestination);
	}

	private bool ShootHookInDirection(PlayerPickupsModule pPickupsModule, Vector3 tDirection, out Vector3 tHitPos)
	{
		/*		Abort player's movement		*/
		PlayerCharacter pPlayer = pPickupsModule.m_pMaster;

		pPlayer.SetCurrentTileTarget(null);
		pPlayer.ClearInputsTileTarget();


		RaycastHit tHit;
		if (!Physics.Raycast(pPickupsModule.m_pMaster.transform.position + (Vector3.up * 0.5f), tDirection, out tHit, m_fMaxShootDistance, LayerMask.GetMask("Wall", "Ghost"), QueryTriggerInteraction.Collide))
		{
			tHitPos = pPickupsModule.m_pMaster.transform.position + (tDirection * m_fMaxShootDistance);
			return false;
		}

		tHitPos = tHit.point - (Vector3.up * 0.5f);

		if (tHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			Tile pActualTileDestination = MapManager.Instance.GetTileFromPosition(tHitPos - (tDirection * 0.1f));
			Tile pCurrentTile = MapManager.Instance.GetTileFromPosition(pPickupsModule.m_pMaster.transform.position);

			if (pActualTileDestination == pCurrentTile)
				return false;

			tHitPos = pActualTileDestination.transform.position;
		}
		else
		{
			Ghost pHitGhost = tHit.collider.GetComponent<Ghost>();
			Assert.IsTrue(pHitGhost != null, "The grappling hook hit something (" + tHit.collider.name + ") that is not a wall but not a ghost either? Something's wrong!");

			pHitGhost.SetBehaviourFrozen(true);

			tHitPos += (tDirection * 0.6f);	// To make sure we get through without dying first
		}

		PinHookToDestination(pPickupsModule.m_pMaster, tHitPos);

		return true;
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

	#endregion

#region Abort

	private void AbortShot(PlayerPickupsModule pPickupsModule, Vector3 tHookDestination)
	{
		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.ABORTED);

		PinHookToDestination(pPickupsModule.m_pMaster, tHookDestination);	// Just show the hook for a frame, for visual feedback on why it's got aborted
	}

	private void ResetVariablesAfterAbort(PlayerPickupsModule pPickupsModule)
	{
		GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
		PutHookBackInBelt(pHook, pPickupsModule.m_pMaster);

		ResetVariables(pPickupsModule);
		pPickupsModule.SetVariable(c_sVariableName_pGrapplingHookObject, pHook);


		pPickupsModule.m_pMaster.SetBehaviourFrozen(false);
		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.AWAITING_INPUT);
	}

	private void PutHookBackInBelt(GrapplingHook pHook, PlayerCharacter pPlayer)
	{
		pHook.m_pRopeEnd.SetParent(pHook.m_pRopeObject.transform);
		pHook.m_pRopeEnd.localPosition = Vector3.zero;
		pHook.m_pRopeObject.SetActive(false);

		pPlayer.m_pInventorySlotsModule.EquipItemInSlot(pHook.transform, EInventorySlot.BELT, false);
		pHook.transform.localPosition = m_pHookPrefab.transform.localPosition;
		pHook.transform.localRotation = m_pHookPrefab.transform.localRotation;
	}

	#endregion

#region Traction

	private void InitTraction(PlayerPickupsModule pPickupsModule, Vector3 tHookDestination)
	{
		PlayerCharacter pPlayer = pPickupsModule.m_pMaster;
		pPlayer.CanKillGhosts = true;
		m_pSuperGhostKillEffect.HookEffect(pPickupsModule.m_pMaster);

		pPickupsModule.SetVariable(c_sVariableName_tGrapplingHookTractionDestination, tHookDestination);
		pPickupsModule.SetVariable(c_sVariableName_fGrapplingHookCurrentStepStartTime, Time.time);

		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.TRACTION);
	}

	private void UpdateTraction(PlayerPickupsModule pPickupsModule)
	{
		float fHookingTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fGrapplingHookCurrentStepStartTime, 0.0f));

		if (Time.time - fHookingTime < m_fTimeBetweenHookingAndTractionInSeconds)
			return;		// TODO: Add recoil here before traction

		Transform pPlayerTransform = pPickupsModule.m_pMaster.transform;

		Vector3 tMoveThisFrame = pPlayerTransform.forward * (m_fTractionSpeed * Time.deltaTime);

		Vector3 tHookHeadPos = (Vector3)(pPickupsModule.GetVariable(c_sVariableName_tGrapplingHookTractionDestination));
		Vector3 tHookToPlayer = pPlayerTransform.position - tHookHeadPos;

		if (tHookToPlayer.sqrMagnitude <= tMoveThisFrame.sqrMagnitude)
		{
			pPlayerTransform.position = tHookHeadPos;
			EffectEnd(pPickupsModule);
		}
		else
		{
			pPlayerTransform.position += tMoveThisFrame;
		}
	}

	#endregion


	override public void EffectEnd(PlayerPickupsModule pPickupsModule)
	{
		GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
		Destroy(pHook.gameObject);

		ResetVariables(pPickupsModule);

		m_pSuperGhostKillEffect.DetachEffect(pPickupsModule.m_pMaster);

		pPickupsModule.RemoveActiveEffect(this);
		pPickupsModule.m_pMaster.SetBehaviourFrozen(false);
		pPickupsModule.m_pMaster.CanKillGhosts = false;
	}
}

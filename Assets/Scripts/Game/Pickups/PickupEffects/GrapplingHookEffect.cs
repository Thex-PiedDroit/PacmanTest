
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


	static private readonly float s_fMaxRaycastDistance = 20.0f;
	static private readonly Vector3 s_tInvalidShootDirection = Vector3.down;

	#endregion


#region EffectInitialization

	override public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.GiveActiveEffect(this);

		GrapplingHook pPreviousHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));

		ResetVariables(pPickupsModule);
		InitHookObject(pPickupsModule, pPreviousHook);

		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.AWAITING_INPUT);
	}

	private void InitHookObject(PlayerPickupsModule pPickupsModule, GrapplingHook pPreviousHook)
	{
		GrapplingHook pHook = pPreviousHook ?? Instantiate(m_pHookPrefab);
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
		float fInputPressTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fGrapplingHookCurrentStepStartTime, 0.0f));

		if (Time.time - fInputPressTime < m_fAnticipationDurationInSeconds)
			return;		// TODO: Add hand animation


		Vector3 tShootDirection = Toolkit.QueryMoveDirectionInput();

		if (tShootDirection != Vector3.zero)
			pPickupsModule.m_pMaster.transform.forward = Toolkit.FlattenDirectionOnOneAxis(tShootDirection);
		else
			tShootDirection = pPickupsModule.m_pMaster.transform.forward;


		Vector3 tHookDestination = ShootHookInDirection(pPickupsModule, tShootDirection);

		if (tHookDestination != s_tInvalidShootDirection)
			InitTraction(pPickupsModule, tHookDestination);
		else
			AbortShot(pPickupsModule);
	}

	private Vector3 ShootHookInDirection(PlayerPickupsModule pPickupsModule, Vector3 tDirection)
	{
		RaycastHit tHit;
		if (!Physics.Raycast(pPickupsModule.m_pMaster.transform.position + (Vector3.up * 0.5f), tDirection, out tHit, s_fMaxRaycastDistance, LayerMask.GetMask("Wall", "Ghost"), QueryTriggerInteraction.Collide))
			return s_tInvalidShootDirection;

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

	#endregion

#region Abort

	private void AbortShot(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.SetVariable(c_sVariableName_eGrapplingHookCurrentStep, EGrapplingHookEffectStep.ABORTED);

		PlayerCharacter pPlayer = pPickupsModule.m_pMaster;
		PinHookToDestination(pPlayer, pPlayer.transform.position + (pPlayer.transform.forward * s_fMaxRaycastDistance));	// Just show the hook for a frame, for visual feedback on why it's got aborted
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

		pPlayer.SetCurrentTileTarget(null);
		pPlayer.ClearInputsTileTarget();
		pPlayer.CanKillGhosts = true;

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

			GrapplingHook pHook = (GrapplingHook)(pPickupsModule.GetVariable(c_sVariableName_pGrapplingHookObject));
			Destroy(pHook.gameObject);

			EffectEnd(pPickupsModule);
		}
		else
		{
			pPlayerTransform.position += tMoveThisFrame;
		}
	}

	#endregion


	override protected void EffectEnd(PlayerPickupsModule pPickupsModule)
	{
		ResetVariables(pPickupsModule);

		pPickupsModule.RemoveActiveEffect(this);
		pPickupsModule.m_pMaster.SetBehaviourFrozen(false);
		pPickupsModule.m_pMaster.CanKillGhosts = false;
	}
}

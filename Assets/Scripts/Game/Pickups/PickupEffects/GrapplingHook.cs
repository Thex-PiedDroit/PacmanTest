
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "GrapplingHook", menuName = "PickupEffects/GrapplingHook")]
public class GrapplingHook : PickupEffect
{
#region Variables (public)

	public string m_sShootInputName = "UseGrapplingHook";

	public float m_fAnticipationDurationInSeconds = 0.5f;

	public float m_fTimeBetweenHookingAndTractionInSeconds = 0.1f;

	public float m_fTractionSpeed = 40.0f;

	#endregion

#region Variables (private)

	private const string c_sVariableName_fGrapplingHookUseTime = "fGrapplingHookUseTime";

	private const string c_sVariableName_tGrapplingHookHeadPos = "tGrapplingHookHeadPos";
	private const string c_sVariableName_fGrapplingHookHookingTime = "fGrapplingHookHookingTime";

	private const string c_sVariableName_bGrapplingHookHitWall = "bGrapplingHookHitWall";

	#endregion


	override public void GiveEffectToPlayer(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.GiveActiveEffect(this);

		ResetVariables(pPickupsModule);
	}

	private void ResetVariables(PlayerPickupsModule pPickupsModule)
	{
		pPickupsModule.ResetVariable(c_sVariableName_fGrapplingHookUseTime);
		pPickupsModule.ResetVariable(c_sVariableName_tGrapplingHookHeadPos);
		pPickupsModule.ResetVariable(c_sVariableName_fGrapplingHookHookingTime);
	}

	override public void UpdateEffect(PlayerPickupsModule pPickupsModule)
	{
		float fUseTime = (float)(pPickupsModule.GetVariable(c_sVariableName_fGrapplingHookUseTime, 0.0f));

		if (fUseTime != 0.0f)
		{
			UpdateShot(pPickupsModule, fUseTime);
		}
		else if (Input.GetButtonDown(m_sShootInputName))
		{
			pPickupsModule.SetVariable(c_sVariableName_fGrapplingHookUseTime, Time.time);
			pPickupsModule.m_pMaster.SetBehaviourFrozen(true);
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

		Vector3 tHitPosition = ShootHookForward(pPickupsModule, tShootDirection);
		if (tHitPosition != Vector3.down)
		{
			pPickupsModule.m_pMaster.SetCurrentTileTarget(null);
			pPickupsModule.m_pMaster.ClearInputsTileTarget();
			pPickupsModule.m_pMaster.CanKillGhosts = true;

			pPickupsModule.SetVariable(c_sVariableName_tGrapplingHookHeadPos, tHitPosition);
			pPickupsModule.SetVariable(c_sVariableName_fGrapplingHookHookingTime, Time.time);
		}
	}

	private Vector3 ShootHookForward(PlayerPickupsModule pPickupsModule, Vector3 tDirection)
	{
		const float c_fMaxRaycastDistance = 20.0f;

		RaycastHit tHit;
		if (!Physics.Raycast(pPickupsModule.m_pMaster.transform.position + (Vector3.up * 0.5f), tDirection, out tHit, c_fMaxRaycastDistance, LayerMask.GetMask("Wall", "Ghost"), QueryTriggerInteraction.Collide))
		{
			AbortShot(pPickupsModule);
			return Vector3.down;
		}

		Vector3 tHitPos = tHit.point - (Vector3.up * 0.5f);
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

	private void AbortShot(PlayerPickupsModule pPickupsModule)
	{
		// TODO: Implement this
		EffectEnd(pPickupsModule);
	}

	private void UpdateTraction(PlayerPickupsModule pPickupsModule, float fHookingTime)
	{
		if (Time.time - fHookingTime < m_fTimeBetweenHookingAndTractionInSeconds)
			return;		// TODO: Add recoil here before traction

		Transform pPlayerTransform = pPickupsModule.m_pMaster.transform;

		Vector3 tMoveThisFrame = pPlayerTransform.forward * (m_fTractionSpeed * Time.deltaTime);

		Vector3 tHookHeadPos = (Vector3)(pPickupsModule.GetVariable(c_sVariableName_tGrapplingHookHeadPos));
		if (pPickupsModule.GetVariableAsBool(c_sVariableName_bGrapplingHookHitWall))
			tHookHeadPos -= pPlayerTransform.forward * 0.1f;

		Vector3 tPlayerToHook = pPlayerTransform.position - tHookHeadPos;

		if (tPlayerToHook.sqrMagnitude <= tMoveThisFrame.sqrMagnitude)
		{
			pPlayerTransform.position = tHookHeadPos;

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

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay;

public class AppearanceAnimator : MonoBehaviour
{
	private class AnimationEventCallback
	{
		public AnimationEventReference eventRef;

		public List<Action<AnimationEventReference>> actionList = new List<Action<AnimationEventReference>>();
	}

	private const float AIM_IK_TRANSITION_OUT_DUR = 0.1f;

	private const float AIM_IK_TRANSITION_IN_DUR = 0.2f;

	private const float FOOT_IK_TRANSITION_DUR = 0.1f;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AppearanceIKController appearanceIKController;

	[Tooltip("How far sideways you can aim before the character starts turning.")]
	[SerializeField]
	[Range(0f, 90f)]
	private float horizontalAimLimit = 65f;

	[SerializeField]
	private bool debug;

	private MaterialModifier materialModifier;

	private Dictionary<string, Transform> boneCache = new Dictionary<string, Transform>();

	private string previousInteractorToggleString = string.Empty;

	private float rotationVelocity;

	private List<AnimationEventCallback> animationCallbacks = new List<AnimationEventCallback>();

	private float currentAimIK;

	private float aimIKDampVelo;

	private float currentFootIK;

	private float footIKDampVelo;

	[NonSerialized]
	private float aimIKTransitionInSpeed;

	[NonSerialized]
	private float aimIKTransitionOutSpeed;

	[NonSerialized]
	private float footIKTransitionSpeed;

	[SerializeField]
	private bool lastAdsState;

	[NonSerialized]
	private Transform pelvisBone;

	[NonSerialized]
	private float pelvisRotation;

	[NonSerialized]
	private bool isInAimState;

	[NonSerialized]
	private bool isInReloadState;

	[NonSerialized]
	private bool isInHotswapState;

	[NonSerialized]
	private int turnLayerIndex;

	[NonSerialized]
	private int reloadLayerIndex;

	[NonSerialized]
	private int hotswapLayerIndex;

	public Animator Animator => animator;

	public MaterialModifier MaterialModifier => materialModifier;

	public bool IsInAimState => isInAimState;

	public bool IsInHotswapState => isInHotswapState;

	public float HorizontalAimLimit => horizontalAimLimit;

	private void Awake()
	{
		aimIKTransitionInSpeed = 5f;
		aimIKTransitionOutSpeed = 10f;
		footIKTransitionSpeed = 10f;
		turnLayerIndex = animator.GetLayerIndex("Turns");
		animator.SetLayerWeight(turnLayerIndex, 0f);
		reloadLayerIndex = animator.GetLayerIndex("Reloads");
		hotswapLayerIndex = animator.GetLayerIndex("Hotswap");
	}

	public void SetAnimationState(float rotation, bool moving, bool walking, bool grounded, float slopeAngle, float airTime, float fallTime, Vector2 worldVelocity, float velX, float velY, float velZ, float angularVelocity, float horizontalVelMagnitude, string interactorToggleString, int comboBookmark, bool ghostmode, bool ads, float playerAngleDot, Vector3 aimPoint, bool useIK)
	{
		animator.SetBool("Moving", moving);
		animator.SetBool("Walking", walking);
		animator.SetFloat("SlopeAngle", slopeAngle);
		animator.SetFloat("VelX", velX);
		animator.SetFloat("VelY", velY);
		animator.SetFloat("VelZ", velZ);
		animator.SetFloat("AngularVelocity", angularVelocity);
		animator.SetFloat("HorizVelMagnitude", horizontalVelMagnitude);
		animator.SetInteger("ComboBookmark", comboBookmark);
		animator.SetFloat("PlayerAngleDot", playerAngleDot);
		animator.SetFloat("AirTime", airTime);
		animator.SetFloat("FallTime", fallTime);
		animator.SetBool("Grounded", grounded);
		bool value = ads;
		if (ads && appearanceIKController.LastSlotInUse == 0 && !grounded)
		{
			value = appearanceIKController.AllowIKInAir();
		}
		animator.SetBool("ZLock", value);
		GetCurrentAnimatorStates();
		if (!interactorToggleString.Equals(previousInteractorToggleString))
		{
			if (!string.IsNullOrEmpty(previousInteractorToggleString))
			{
				animator.SetBool(previousInteractorToggleString, value: false);
			}
			if (!string.IsNullOrEmpty(interactorToggleString))
			{
				animator.SetBool(interactorToggleString, value: true);
			}
			previousInteractorToggleString = interactorToggleString;
		}
		float y = base.transform.parent.localEulerAngles.y;
		float num = Mathf.SmoothDampAngle(y, rotation, ref rotationVelocity, ads ? 0f : 0.1f);
		ads &= appearanceIKController.LastSlotInUse == 0 && grounded;
		if (ads)
		{
			if (moving)
			{
				y = Mathf.MoveTowardsAngle(y, rotation, 360f * Time.deltaTime);
				num = y;
				SnapRotation(y);
			}
			else if (Mathf.Abs(Mathf.DeltaAngle(y, num)) > horizontalAimLimit)
			{
				float rotation2 = Mathf.MoveTowardsAngle(num, y, horizontalAimLimit);
				SnapRotation(rotation2);
			}
		}
		else
		{
			SnapRotation(num);
		}
		lastAdsState = ads;
	}

	public void SnapRotation(float rotation)
	{
		base.transform.parent.localEulerAngles = new Vector3(0f, rotation, 0f);
	}

	public void UpdateIK(bool useIK, bool grounded, bool ghostMode, Vector3 aimPoint, Vector3 lookAtPoint)
	{
		if (useIK && (grounded || ghostMode))
		{
			currentAimIK = Mathf.MoveTowards(currentAimIK, 1f, Time.deltaTime * aimIKTransitionInSpeed);
		}
		else
		{
			currentAimIK = Mathf.MoveTowards(currentAimIK, 0f, Time.deltaTime * aimIKTransitionOutSpeed);
		}
		appearanceIKController.SetAimIKWeight(currentAimIK);
		if (grounded)
		{
			currentFootIK = Mathf.MoveTowards(currentFootIK, 1f, Time.deltaTime * footIKTransitionSpeed);
		}
		else
		{
			currentFootIK = Mathf.MoveTowards(currentFootIK, 0f, Time.deltaTime * footIKTransitionSpeed);
		}
		appearanceIKController.SetFootIKWeight(0f);
		appearanceIKController.SetAimPosition(aimPoint);
	}

	public void ResetAnimator()
	{
		animator.Rebind();
		animator.Update(0f);
	}

	public void TriggerAnimation(string trigger)
	{
		animator.SetTrigger(trigger);
	}

	public Transform GetBone(string boneName)
	{
		if (!boneCache.ContainsKey(boneName))
		{
			boneCache.Add(boneName, FindBone(boneName, base.transform));
		}
		return boneCache[boneName];
	}

	public bool TriggerAttackCombo(int comboBookmark)
	{
		if (comboBookmark != animator.GetInteger("ComboBookmark"))
		{
			animator.SetInteger("ComboBookmark", comboBookmark);
			TriggerAnimation("Attack");
			return true;
		}
		return false;
	}

	private Transform FindBone(string boneName, Transform currentTransform)
	{
		if (currentTransform.gameObject.name == boneName)
		{
			return currentTransform;
		}
		int childCount = currentTransform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform transform = FindBone(boneName, currentTransform.GetChild(i));
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	private void GetCurrentAnimatorStates()
	{
		_ = isInAimState;
		_ = isInReloadState;
		isInAimState = animator.GetCurrentAnimatorStateInfo(0).IsTag("Aim") && (!animator.IsInTransition(0) || animator.GetNextAnimatorStateInfo(0).IsTag("Aim"));
		isInReloadState = animator.GetCurrentAnimatorStateInfo(reloadLayerIndex).IsTag("Reload") || animator.GetNextAnimatorStateInfo(reloadLayerIndex).IsTag("Reload");
		isInHotswapState = animator.GetCurrentAnimatorClipInfoCount(hotswapLayerIndex) > 0 || (animator.IsInTransition(hotswapLayerIndex) && animator.GetNextAnimatorClipInfoCount(hotswapLayerIndex) > 0);
	}

	public void InitializeCosmetics()
	{
		if (debug)
		{
			Debug.Log("InitializeCosmetics for " + base.name);
		}
		materialModifier = base.gameObject.AddComponent<MaterialModifier>();
		Animator.Rebind();
		appearanceIKController.Initialize();
		Invoke("ResetWeapon", 1f);
		pelvisBone = GetBone("Rig.Pelvis");
	}

	public void ResetWeapon()
	{
		appearanceIKController.WeaponReset();
	}

	public void InitRotation(float rot)
	{
		base.transform.parent.localEulerAngles = new Vector3(0f, rot, 0f);
	}

	public void AnimationEvent(UnityEngine.Object value)
	{
		if (!(value != null))
		{
			return;
		}
		AnimationEventReference evt = value as AnimationEventReference;
		if ((object)evt == null)
		{
			return;
		}
		AnimationEventCallback animationEventCallback = animationCallbacks.Find((AnimationEventCallback x) => x.eventRef == evt);
		if (animationEventCallback == null)
		{
			return;
		}
		if (debug)
		{
			Debug.Log($"Anim event for {value.name}, list has {animationEventCallback.actionList.Count} items.");
		}
		foreach (Action<AnimationEventReference> action in animationEventCallback.actionList)
		{
			action(evt);
		}
	}

	public void RegisterAnimationEventCallback(AnimationEventReference reference, Action<AnimationEventReference> callback)
	{
		AnimationEventCallback animationEventCallback = animationCallbacks.Find((AnimationEventCallback x) => x.eventRef == reference);
		if (animationEventCallback == null)
		{
			animationEventCallback = new AnimationEventCallback
			{
				eventRef = reference,
				actionList = { callback }
			};
			animationCallbacks.Add(animationEventCallback);
			if (debug)
			{
				Debug.Log("New callback registered for event " + reference.name + ": " + callback.Method.Name);
			}
		}
		else
		{
			animationEventCallback.actionList.Add(callback);
			if (debug)
			{
				Debug.Log("Callback registered for event " + reference.name + ": " + callback.Method.Name);
			}
		}
	}

	public void RemoveAnimationEventCallback(AnimationEventReference reference, Action<AnimationEventReference> callback)
	{
		AnimationEventCallback animationEventCallback = animationCallbacks.Find((AnimationEventCallback x) => x.eventRef == reference);
		if (animationEventCallback != null)
		{
			animationEventCallback.actionList.RemoveSwapBack(callback);
			if (debug)
			{
				Debug.Log("Callback removed from event " + reference.name + ": " + callback.Method.Name);
			}
		}
	}

	public void OnEquippedItemChanged(Item weaponItem, bool equipped)
	{
		appearanceIKController.OnEquippedItemChanged(weaponItem, equipped);
	}

	public void SetActiveEquipmentSlot(int slot)
	{
		appearanceIKController.SetActiveEquipmentSlot(slot);
	}

	public bool IsUsingAimIK()
	{
		return appearanceIKController.IsUsingAimIK();
	}

	public bool GetAimIKForward(out Vector3 forward)
	{
		return appearanceIKController.GetAimIKForward(out forward);
	}
}

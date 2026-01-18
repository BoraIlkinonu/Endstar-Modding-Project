using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000058 RID: 88
	public class AppearanceAnimator : MonoBehaviour
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600014C RID: 332 RVA: 0x00007A05 File Offset: 0x00005C05
		public Animator Animator
		{
			get
			{
				return this.animator;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600014D RID: 333 RVA: 0x00007A0D File Offset: 0x00005C0D
		public MaterialModifier MaterialModifier
		{
			get
			{
				return this.materialModifier;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600014E RID: 334 RVA: 0x00007A15 File Offset: 0x00005C15
		public bool IsInAimState
		{
			get
			{
				return this.isInAimState;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600014F RID: 335 RVA: 0x00007A1D File Offset: 0x00005C1D
		public bool IsInHotswapState
		{
			get
			{
				return this.isInHotswapState;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000150 RID: 336 RVA: 0x00007A25 File Offset: 0x00005C25
		public float HorizontalAimLimit
		{
			get
			{
				return this.horizontalAimLimit;
			}
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00007A30 File Offset: 0x00005C30
		private void Awake()
		{
			this.aimIKTransitionInSpeed = 5f;
			this.aimIKTransitionOutSpeed = 10f;
			this.footIKTransitionSpeed = 10f;
			this.turnLayerIndex = this.animator.GetLayerIndex("Turns");
			this.animator.SetLayerWeight(this.turnLayerIndex, 0f);
			this.reloadLayerIndex = this.animator.GetLayerIndex("Reloads");
			this.hotswapLayerIndex = this.animator.GetLayerIndex("Hotswap");
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00007AB8 File Offset: 0x00005CB8
		public void SetAnimationState(float rotation, bool moving, bool walking, bool grounded, float slopeAngle, float airTime, float fallTime, Vector2 worldVelocity, float velX, float velY, float velZ, float angularVelocity, float horizontalVelMagnitude, string interactorToggleString, int comboBookmark, bool ghostmode, bool ads, float playerAngleDot, Vector3 aimPoint, bool useIK)
		{
			this.animator.SetBool("Moving", moving);
			this.animator.SetBool("Walking", walking);
			this.animator.SetFloat("SlopeAngle", slopeAngle);
			this.animator.SetFloat("VelX", velX);
			this.animator.SetFloat("VelY", velY);
			this.animator.SetFloat("VelZ", velZ);
			this.animator.SetFloat("AngularVelocity", angularVelocity);
			this.animator.SetFloat("HorizVelMagnitude", horizontalVelMagnitude);
			this.animator.SetInteger("ComboBookmark", comboBookmark);
			this.animator.SetFloat("PlayerAngleDot", playerAngleDot);
			this.animator.SetFloat("AirTime", airTime);
			this.animator.SetFloat("FallTime", fallTime);
			this.animator.SetBool("Grounded", grounded);
			bool flag = ads;
			if (ads && this.appearanceIKController.LastSlotInUse == 0 && !grounded)
			{
				flag = this.appearanceIKController.AllowIKInAir();
			}
			this.animator.SetBool("ZLock", flag);
			this.GetCurrentAnimatorStates();
			if (!interactorToggleString.Equals(this.previousInteractorToggleString))
			{
				if (!string.IsNullOrEmpty(this.previousInteractorToggleString))
				{
					this.animator.SetBool(this.previousInteractorToggleString, false);
				}
				if (!string.IsNullOrEmpty(interactorToggleString))
				{
					this.animator.SetBool(interactorToggleString, true);
				}
				this.previousInteractorToggleString = interactorToggleString;
			}
			float num = base.transform.parent.localEulerAngles.y;
			float num2 = Mathf.SmoothDampAngle(num, rotation, ref this.rotationVelocity, ads ? 0f : 0.1f);
			ads &= this.appearanceIKController.LastSlotInUse == 0 && grounded;
			if (ads)
			{
				if (moving)
				{
					num = Mathf.MoveTowardsAngle(num, rotation, 360f * Time.deltaTime);
					this.SnapRotation(num);
				}
				else if (Mathf.Abs(Mathf.DeltaAngle(num, num2)) > this.horizontalAimLimit)
				{
					float num3 = Mathf.MoveTowardsAngle(num2, num, this.horizontalAimLimit);
					this.SnapRotation(num3);
				}
			}
			else
			{
				this.SnapRotation(num2);
			}
			this.lastAdsState = ads;
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00007CDE File Offset: 0x00005EDE
		public void SnapRotation(float rotation)
		{
			base.transform.parent.localEulerAngles = new Vector3(0f, rotation, 0f);
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00007D00 File Offset: 0x00005F00
		public void UpdateIK(bool useIK, bool grounded, bool ghostMode, Vector3 aimPoint, Vector3 lookAtPoint)
		{
			if (useIK && (grounded || ghostMode))
			{
				this.currentAimIK = Mathf.MoveTowards(this.currentAimIK, 1f, Time.deltaTime * this.aimIKTransitionInSpeed);
			}
			else
			{
				this.currentAimIK = Mathf.MoveTowards(this.currentAimIK, 0f, Time.deltaTime * this.aimIKTransitionOutSpeed);
			}
			this.appearanceIKController.SetAimIKWeight(this.currentAimIK);
			if (grounded)
			{
				this.currentFootIK = Mathf.MoveTowards(this.currentFootIK, 1f, Time.deltaTime * this.footIKTransitionSpeed);
			}
			else
			{
				this.currentFootIK = Mathf.MoveTowards(this.currentFootIK, 0f, Time.deltaTime * this.footIKTransitionSpeed);
			}
			this.appearanceIKController.SetFootIKWeight(0f);
			this.appearanceIKController.SetAimPosition(aimPoint);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x00007DD5 File Offset: 0x00005FD5
		public void ResetAnimator()
		{
			this.animator.Rebind();
			this.animator.Update(0f);
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00007DF2 File Offset: 0x00005FF2
		public void TriggerAnimation(string trigger)
		{
			this.animator.SetTrigger(trigger);
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00007E00 File Offset: 0x00006000
		public Transform GetBone(string boneName)
		{
			if (!this.boneCache.ContainsKey(boneName))
			{
				this.boneCache.Add(boneName, this.FindBone(boneName, base.transform));
			}
			return this.boneCache[boneName];
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00007E35 File Offset: 0x00006035
		public bool TriggerAttackCombo(int comboBookmark)
		{
			if (comboBookmark != this.animator.GetInteger("ComboBookmark"))
			{
				this.animator.SetInteger("ComboBookmark", comboBookmark);
				this.TriggerAnimation("Attack");
				return true;
			}
			return false;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00007E6C File Offset: 0x0000606C
		private Transform FindBone(string boneName, Transform currentTransform)
		{
			if (currentTransform.gameObject.name == boneName)
			{
				return currentTransform;
			}
			int childCount = currentTransform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform transform = this.FindBone(boneName, currentTransform.GetChild(i));
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00007EBC File Offset: 0x000060BC
		private void GetCurrentAnimatorStates()
		{
			bool flag = this.isInAimState;
			bool flag2 = this.isInReloadState;
			this.isInAimState = this.animator.GetCurrentAnimatorStateInfo(0).IsTag("Aim") && (!this.animator.IsInTransition(0) || this.animator.GetNextAnimatorStateInfo(0).IsTag("Aim"));
			this.isInReloadState = this.animator.GetCurrentAnimatorStateInfo(this.reloadLayerIndex).IsTag("Reload") || this.animator.GetNextAnimatorStateInfo(this.reloadLayerIndex).IsTag("Reload");
			this.isInHotswapState = this.animator.GetCurrentAnimatorClipInfoCount(this.hotswapLayerIndex) > 0 || (this.animator.IsInTransition(this.hotswapLayerIndex) && this.animator.GetNextAnimatorClipInfoCount(this.hotswapLayerIndex) > 0);
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00007FB4 File Offset: 0x000061B4
		public void InitializeCosmetics()
		{
			if (this.debug)
			{
				Debug.Log("InitializeCosmetics for " + base.name);
			}
			this.materialModifier = base.gameObject.AddComponent<MaterialModifier>();
			this.Animator.Rebind();
			this.appearanceIKController.Initialize();
			base.Invoke("ResetWeapon", 1f);
			this.pelvisBone = this.GetBone("Rig.Pelvis");
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00008026 File Offset: 0x00006226
		public void ResetWeapon()
		{
			this.appearanceIKController.WeaponReset();
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00007CDE File Offset: 0x00005EDE
		public void InitRotation(float rot)
		{
			base.transform.parent.localEulerAngles = new Vector3(0f, rot, 0f);
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00008034 File Offset: 0x00006234
		public void AnimationEvent(global::UnityEngine.Object value)
		{
			if (value != null)
			{
				AnimationEventReference evt = value as AnimationEventReference;
				if (evt != null)
				{
					AppearanceAnimator.AnimationEventCallback animationEventCallback = this.animationCallbacks.Find((AppearanceAnimator.AnimationEventCallback x) => x.eventRef == evt);
					if (animationEventCallback != null)
					{
						if (this.debug)
						{
							Debug.Log(string.Format("Anim event for {0}, list has {1} items.", value.name, animationEventCallback.actionList.Count));
						}
						foreach (Action<AnimationEventReference> action in animationEventCallback.actionList)
						{
							action(evt);
						}
					}
				}
			}
		}

		// Token: 0x0600015F RID: 351 RVA: 0x000080FC File Offset: 0x000062FC
		public void RegisterAnimationEventCallback(AnimationEventReference reference, Action<AnimationEventReference> callback)
		{
			AppearanceAnimator.AnimationEventCallback animationEventCallback = this.animationCallbacks.Find((AppearanceAnimator.AnimationEventCallback x) => x.eventRef == reference);
			if (animationEventCallback == null)
			{
				animationEventCallback = new AppearanceAnimator.AnimationEventCallback
				{
					eventRef = reference,
					actionList = { callback }
				};
				this.animationCallbacks.Add(animationEventCallback);
				if (this.debug)
				{
					Debug.Log("New callback registered for event " + reference.name + ": " + callback.Method.Name);
					return;
				}
			}
			else
			{
				animationEventCallback.actionList.Add(callback);
				if (this.debug)
				{
					Debug.Log("Callback registered for event " + reference.name + ": " + callback.Method.Name);
				}
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x000081CC File Offset: 0x000063CC
		public void RemoveAnimationEventCallback(AnimationEventReference reference, Action<AnimationEventReference> callback)
		{
			AppearanceAnimator.AnimationEventCallback animationEventCallback = this.animationCallbacks.Find((AppearanceAnimator.AnimationEventCallback x) => x.eventRef == reference);
			if (animationEventCallback != null)
			{
				animationEventCallback.actionList.RemoveSwapBack(callback);
				if (this.debug)
				{
					Debug.Log("Callback removed from event " + reference.name + ": " + callback.Method.Name);
				}
			}
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00008240 File Offset: 0x00006440
		public void OnEquippedItemChanged(Item weaponItem, bool equipped)
		{
			this.appearanceIKController.OnEquippedItemChanged(weaponItem, equipped);
		}

		// Token: 0x06000162 RID: 354 RVA: 0x0000824F File Offset: 0x0000644F
		public void SetActiveEquipmentSlot(int slot)
		{
			this.appearanceIKController.SetActiveEquipmentSlot(slot);
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000825D File Offset: 0x0000645D
		public bool IsUsingAimIK()
		{
			return this.appearanceIKController.IsUsingAimIK();
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000826A File Offset: 0x0000646A
		public bool GetAimIKForward(out Vector3 forward)
		{
			return this.appearanceIKController.GetAimIKForward(out forward);
		}

		// Token: 0x04000116 RID: 278
		private const float AIM_IK_TRANSITION_OUT_DUR = 0.1f;

		// Token: 0x04000117 RID: 279
		private const float AIM_IK_TRANSITION_IN_DUR = 0.2f;

		// Token: 0x04000118 RID: 280
		private const float FOOT_IK_TRANSITION_DUR = 0.1f;

		// Token: 0x04000119 RID: 281
		[SerializeField]
		private Animator animator;

		// Token: 0x0400011A RID: 282
		[SerializeField]
		private AppearanceIKController appearanceIKController;

		// Token: 0x0400011B RID: 283
		[Tooltip("How far sideways you can aim before the character starts turning.")]
		[SerializeField]
		[Range(0f, 90f)]
		private float horizontalAimLimit = 65f;

		// Token: 0x0400011C RID: 284
		[SerializeField]
		private bool debug;

		// Token: 0x0400011D RID: 285
		private MaterialModifier materialModifier;

		// Token: 0x0400011E RID: 286
		private Dictionary<string, Transform> boneCache = new Dictionary<string, Transform>();

		// Token: 0x0400011F RID: 287
		private string previousInteractorToggleString = string.Empty;

		// Token: 0x04000120 RID: 288
		private float rotationVelocity;

		// Token: 0x04000121 RID: 289
		private List<AppearanceAnimator.AnimationEventCallback> animationCallbacks = new List<AppearanceAnimator.AnimationEventCallback>();

		// Token: 0x04000122 RID: 290
		private float currentAimIK;

		// Token: 0x04000123 RID: 291
		private float aimIKDampVelo;

		// Token: 0x04000124 RID: 292
		private float currentFootIK;

		// Token: 0x04000125 RID: 293
		private float footIKDampVelo;

		// Token: 0x04000126 RID: 294
		[NonSerialized]
		private float aimIKTransitionInSpeed;

		// Token: 0x04000127 RID: 295
		[NonSerialized]
		private float aimIKTransitionOutSpeed;

		// Token: 0x04000128 RID: 296
		[NonSerialized]
		private float footIKTransitionSpeed;

		// Token: 0x04000129 RID: 297
		[SerializeField]
		private bool lastAdsState;

		// Token: 0x0400012A RID: 298
		[NonSerialized]
		private Transform pelvisBone;

		// Token: 0x0400012B RID: 299
		[NonSerialized]
		private float pelvisRotation;

		// Token: 0x0400012C RID: 300
		[NonSerialized]
		private bool isInAimState;

		// Token: 0x0400012D RID: 301
		[NonSerialized]
		private bool isInReloadState;

		// Token: 0x0400012E RID: 302
		[NonSerialized]
		private bool isInHotswapState;

		// Token: 0x0400012F RID: 303
		[NonSerialized]
		private int turnLayerIndex;

		// Token: 0x04000130 RID: 304
		[NonSerialized]
		private int reloadLayerIndex;

		// Token: 0x04000131 RID: 305
		[NonSerialized]
		private int hotswapLayerIndex;

		// Token: 0x02000059 RID: 89
		private class AnimationEventCallback
		{
			// Token: 0x04000132 RID: 306
			public AnimationEventReference eventRef;

			// Token: 0x04000133 RID: 307
			public List<Action<AnimationEventReference>> actionList = new List<Action<AnimationEventReference>>();
		}
	}
}

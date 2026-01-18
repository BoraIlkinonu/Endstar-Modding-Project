using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000FC RID: 252
	public class MeleeAttackData : MonoBehaviour
	{
		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000596 RID: 1430 RVA: 0x0001C3E0 File Offset: 0x0001A5E0
		public List<MeleeAttackData.FrameData> Frames
		{
			get
			{
				return this.frames;
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000597 RID: 1431 RVA: 0x0001C3E8 File Offset: 0x0001A5E8
		public AnimationClip Clip
		{
			get
			{
				return this.clip;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x06000598 RID: 1432 RVA: 0x0001C3F0 File Offset: 0x0001A5F0
		public Animator PreviewAnimator
		{
			get
			{
				return this.previewAnimator;
			}
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x06000599 RID: 1433 RVA: 0x0001C3F8 File Offset: 0x0001A5F8
		public int AttackAnimationFrameCount
		{
			get
			{
				return this.Frames.Count;
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x0600059A RID: 1434 RVA: 0x0001C405 File Offset: 0x0001A605
		public int TotalAttackFrameCount
		{
			get
			{
				return this.AttackAnimationFrameCount + this.CooldownFrames;
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x0600059B RID: 1435 RVA: 0x0001C414 File Offset: 0x0001A614
		public int AttackMovementForce
		{
			get
			{
				return this.attackMovementForce;
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x0600059C RID: 1436 RVA: 0x0001C41C File Offset: 0x0001A61C
		public AnimationCurve AttackMovementForceCurve
		{
			get
			{
				return this.attackMovementForceCurve;
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x0600059D RID: 1437 RVA: 0x0001C424 File Offset: 0x0001A624
		public int HitDamage
		{
			get
			{
				return this.hitDamage;
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x0001C42C File Offset: 0x0001A62C
		public int CooldownFrames
		{
			get
			{
				return this.cooldownFrames;
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x0600059F RID: 1439 RVA: 0x0001C434 File Offset: 0x0001A634
		public float TrackingSpeed
		{
			get
			{
				return this.trackingSpeed;
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060005A0 RID: 1440 RVA: 0x0001C43C File Offset: 0x0001A63C
		public float TrackingHeight
		{
			get
			{
				return this.trackingHeight;
			}
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060005A1 RID: 1441 RVA: 0x0001C444 File Offset: 0x0001A644
		public float TrackingAngle
		{
			get
			{
				return this.trackingAngle;
			}
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060005A2 RID: 1442 RVA: 0x0001C44C File Offset: 0x0001A64C
		public float TargetDistance
		{
			get
			{
				return this.targetDistance;
			}
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060005A3 RID: 1443 RVA: 0x0001C454 File Offset: 0x0001A654
		public float MaxTrackingDistance
		{
			get
			{
				return this.maxTrackingDistance;
			}
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x0001C45C File Offset: 0x0001A65C
		public float KnockbackForce
		{
			get
			{
				return this.knockbackForce;
			}
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060005A5 RID: 1445 RVA: 0x0001C464 File Offset: 0x0001A664
		public uint KnockbackFrames
		{
			get
			{
				return this.knockbackFrames;
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060005A6 RID: 1446 RVA: 0x0001C46C File Offset: 0x0001A66C
		public MeleeVfxPlayer MeleeVFXPrefab
		{
			get
			{
				return this.meleeVfxPrefab;
			}
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060005A7 RID: 1447 RVA: 0x0001C474 File Offset: 0x0001A674
		public float VFXdelay
		{
			get
			{
				return this.vfxDelay;
			}
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x0001C47C File Offset: 0x0001A67C
		public HashSet<HittableComponent> CheckCollisions(uint frame, Vector3 position, float rotation)
		{
			base.transform.position = position;
			base.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
			HashSet<HittableComponent> hashSet = new HashSet<HittableComponent>();
			if ((long)this.frames.Count > (long)((ulong)frame))
			{
				for (int i = 0; i < this.allHurtBoxInfo.Count; i++)
				{
					if (this.frames[(int)frame].HurtBoxesActiveThisFrame[i])
					{
						int overlaps = this.GetOverlaps(this.allHurtBoxInfo[i]);
						for (int j = 0; j < overlaps; j++)
						{
							HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(this.overlapColliders[j]);
							if (hittableFromMap != null)
							{
								hashSet.Add(hittableFromMap);
							}
						}
					}
				}
				return hashSet;
			}
			return hashSet;
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x0001C540 File Offset: 0x0001A740
		private int GetOverlaps(MeleeAttackData.HurtBoxInfo hurtBox)
		{
			return Physics.OverlapBoxNonAlloc(base.transform.TransformPoint(hurtBox.position), hurtBox.scale / 2f, this.overlapColliders, hurtBox.rotation * base.transform.rotation, 1 << LayerMask.NameToLayer("HittableColliders"), QueryTriggerInteraction.Ignore);
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x0001C59F File Offset: 0x0001A79F
		public void RuntimeSetup()
		{
			this.previewAnimator.gameObject.SetActive(false);
		}

		// Token: 0x0400042C RID: 1068
		[Header("Baking")]
		[SerializeField]
		private AnimationClip clip;

		// Token: 0x0400042D RID: 1069
		[SerializeField]
		private Animator previewAnimator;

		// Token: 0x0400042E RID: 1070
		[Header("Runtime")]
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x0400042F RID: 1071
		[SerializeField]
		private int attackMovementForce;

		// Token: 0x04000430 RID: 1072
		[SerializeField]
		private AnimationCurve attackMovementForceCurve;

		// Token: 0x04000431 RID: 1073
		[SerializeField]
		private int hitDamage;

		// Token: 0x04000432 RID: 1074
		[SerializeField]
		private int cooldownFrames;

		// Token: 0x04000433 RID: 1075
		[Header("VFX")]
		[SerializeField]
		private MeleeVfxPlayer meleeVfxPrefab;

		// Token: 0x04000434 RID: 1076
		[SerializeField]
		[Tooltip("Delay (in seconds) before starting VFX when attack animation triggers.")]
		private float vfxDelay;

		// Token: 0x04000435 RID: 1077
		[Header("Tracking")]
		[SerializeField]
		private float trackingSpeed = 10f;

		// Token: 0x04000436 RID: 1078
		[SerializeField]
		private float trackingHeight = 0.8f;

		// Token: 0x04000437 RID: 1079
		[SerializeField]
		[Range(0f, 180f)]
		private float trackingAngle = 50f;

		// Token: 0x04000438 RID: 1080
		[SerializeField]
		private float targetDistance;

		// Token: 0x04000439 RID: 1081
		[SerializeField]
		public float maxTrackingDistance;

		// Token: 0x0400043A RID: 1082
		[Header("Knockback")]
		[SerializeField]
		private float knockbackForce = 7.5f;

		// Token: 0x0400043B RID: 1083
		[SerializeField]
		private uint knockbackFrames = 6U;

		// Token: 0x0400043C RID: 1084
		[Header("Frames")]
		[SerializeField]
		[HideInInspector]
		public List<MeleeAttackData.FrameData> frames;

		// Token: 0x0400043D RID: 1085
		[SerializeField]
		private List<MeleeAttackData.HurtBoxInfo> allHurtBoxInfo = new List<MeleeAttackData.HurtBoxInfo>();

		// Token: 0x0400043E RID: 1086
		private readonly Collider[] overlapColliders = new Collider[10];

		// Token: 0x020000FD RID: 253
		[Serializable]
		public struct FrameData
		{
			// Token: 0x0400043F RID: 1087
			public List<bool> HurtBoxesActiveThisFrame;
		}

		// Token: 0x020000FE RID: 254
		[Serializable]
		public class HurtBoxInfo
		{
			// Token: 0x060005AC RID: 1452 RVA: 0x0001C612 File Offset: 0x0001A812
			public Matrix4x4 Matrix(Transform transform)
			{
				return Matrix4x4.TRS(this.position, this.rotation, this.scale);
			}

			// Token: 0x04000440 RID: 1088
			public Vector3 position;

			// Token: 0x04000441 RID: 1089
			public Quaternion rotation;

			// Token: 0x04000442 RID: 1090
			public Vector3 scale;

			// Token: 0x04000443 RID: 1091
			[HideInInspector]
			public bool activePreview;
		}
	}
}

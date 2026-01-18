using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class MeleeAttackData : MonoBehaviour
{
	[Serializable]
	public struct FrameData
	{
		public List<bool> HurtBoxesActiveThisFrame;
	}

	[Serializable]
	public class HurtBoxInfo
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		[HideInInspector]
		public bool activePreview;

		public Matrix4x4 Matrix(Transform transform)
		{
			return Matrix4x4.TRS(position, rotation, scale);
		}
	}

	[Header("Baking")]
	[SerializeField]
	private AnimationClip clip;

	[SerializeField]
	private Animator previewAnimator;

	[Header("Runtime")]
	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private int attackMovementForce;

	[SerializeField]
	private AnimationCurve attackMovementForceCurve;

	[SerializeField]
	private int hitDamage;

	[SerializeField]
	private int cooldownFrames;

	[Header("VFX")]
	[SerializeField]
	private MeleeVfxPlayer meleeVfxPrefab;

	[SerializeField]
	[Tooltip("Delay (in seconds) before starting VFX when attack animation triggers.")]
	private float vfxDelay;

	[Header("Tracking")]
	[SerializeField]
	private float trackingSpeed = 10f;

	[SerializeField]
	private float trackingHeight = 0.8f;

	[SerializeField]
	[Range(0f, 180f)]
	private float trackingAngle = 50f;

	[SerializeField]
	private float targetDistance;

	[SerializeField]
	public float maxTrackingDistance;

	[Header("Knockback")]
	[SerializeField]
	private float knockbackForce = 7.5f;

	[SerializeField]
	private uint knockbackFrames = 6u;

	[Header("Frames")]
	[SerializeField]
	[HideInInspector]
	public List<FrameData> frames;

	[SerializeField]
	private List<HurtBoxInfo> allHurtBoxInfo = new List<HurtBoxInfo>();

	private readonly Collider[] overlapColliders = new Collider[10];

	public List<FrameData> Frames => frames;

	public AnimationClip Clip => clip;

	public Animator PreviewAnimator => previewAnimator;

	public int AttackAnimationFrameCount => Frames.Count;

	public int TotalAttackFrameCount => AttackAnimationFrameCount + CooldownFrames;

	public int AttackMovementForce => attackMovementForce;

	public AnimationCurve AttackMovementForceCurve => attackMovementForceCurve;

	public int HitDamage => hitDamage;

	public int CooldownFrames => cooldownFrames;

	public float TrackingSpeed => trackingSpeed;

	public float TrackingHeight => trackingHeight;

	public float TrackingAngle => trackingAngle;

	public float TargetDistance => targetDistance;

	public float MaxTrackingDistance => maxTrackingDistance;

	public float KnockbackForce => knockbackForce;

	public uint KnockbackFrames => knockbackFrames;

	public MeleeVfxPlayer MeleeVFXPrefab => meleeVfxPrefab;

	public float VFXdelay => vfxDelay;

	public HashSet<HittableComponent> CheckCollisions(uint frame, Vector3 position, float rotation)
	{
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
		HashSet<HittableComponent> hashSet = new HashSet<HittableComponent>();
		if (frames.Count > frame)
		{
			for (int i = 0; i < allHurtBoxInfo.Count; i++)
			{
				if (!frames[(int)frame].HurtBoxesActiveThisFrame[i])
				{
					continue;
				}
				int overlaps = GetOverlaps(allHurtBoxInfo[i]);
				for (int j = 0; j < overlaps; j++)
				{
					HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(overlapColliders[j]);
					if (hittableFromMap != null)
					{
						hashSet.Add(hittableFromMap);
					}
				}
			}
			return hashSet;
		}
		return hashSet;
	}

	private int GetOverlaps(HurtBoxInfo hurtBox)
	{
		return Physics.OverlapBoxNonAlloc(base.transform.TransformPoint(hurtBox.position), hurtBox.scale / 2f, overlapColliders, hurtBox.rotation * base.transform.rotation, 1 << LayerMask.NameToLayer("HittableColliders"), QueryTriggerInteraction.Ignore);
	}

	public void RuntimeSetup()
	{
		previewAnimator.gameObject.SetActive(value: false);
	}
}

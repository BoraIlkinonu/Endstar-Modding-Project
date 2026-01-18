using System;
using Endless.Gameplay.PlayerInventory;
using Endless.Shared.DataTypes;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

namespace Endless.Gameplay;

public class AppearanceIKController : MonoBehaviour
{
	[Flags]
	public enum IKMode : byte
	{
		None = 0,
		Look = 1,
		Aim = 2,
		Ground = 4,
		FullBody = 8
	}

	private const string GUN_AIM_TRANSFORM_NAME = "Barrel_Point";

	[SerializeField]
	private FullBodyBipedIK fullBodyBipedIK;

	[SerializeField]
	private GrounderFBBIK grounderBipedIK;

	[SerializeField]
	private AimIK aimIK;

	[SerializeField]
	private LookAtIK lookAtIK;

	[SerializeField]
	private BipedReferences references;

	[SerializeField]
	private float lookAtWeight = 0.943f;

	[SerializeField]
	private float lookAtHeadWeight = 0.891f;

	[SerializeField]
	private AnimationCurve lookAtSpineWeight = new AnimationCurve();

	[NonSerialized]
	private Transform aimTarget;

	[NonSerialized]
	private Transform lookTarget;

	[NonSerialized]
	private Vector3 defaultAimAxis = Vector3.forward;

	[NonSerialized]
	private IKMode ikMode;

	[SerializeField]
	private bool updateAimAxis;

	[NonSerialized]
	private SerializableGuid equippedGuid;

	[NonSerialized]
	private Item majorItem;

	[NonSerialized]
	private Item minorItem;

	[NonSerialized]
	private RangedWeaponItem rangedMajorItem;

	[NonSerialized]
	private int lastSlotInUse = -2;

	[SerializeField]
	private Vector3 leftHandAxis = Vector3.left;

	[NonSerialized]
	private Action solverUpdate = delegate
	{
	};

	public BipedReferences References => references;

	public Transform RootNode => references.spine[0];

	public IKMode CurrentIKMode => ikMode;

	public int LastSlotInUse => lastSlotInUse;

	private void Awake()
	{
		LookAtIK obj = lookAtIK;
		AimIK obj2 = aimIK;
		GrounderFBBIK grounderFBBIK = grounderBipedIK;
		bool flag = (fullBodyBipedIK.enabled = false);
		bool flag3 = (grounderFBBIK.enabled = flag);
		bool flag5 = (obj2.enabled = flag3);
		obj.enabled = flag5;
		SetIKMode(IKMode.None);
		updateAimAxis = true;
	}

	private void OnDestroy()
	{
		if (aimTarget != null)
		{
			UnityEngine.Object.Destroy(aimTarget.gameObject);
		}
		if (lookTarget != null)
		{
			UnityEngine.Object.Destroy(lookTarget.gameObject);
		}
	}

	private void Update()
	{
		aimIK.solver.FixTransforms();
	}

	private void LateUpdate()
	{
		if (aimIK == null || aimIK.solver == null)
		{
			Debug.LogError($"aimik null? {aimIK == null} solver? {aimIK?.solver == null}");
			return;
		}
		if (updateAimAxis && aimIK.solver.transform != null)
		{
			aimIK.solver.axis = aimIK.solver.transform.InverseTransformDirection(base.transform.forward);
		}
		solverUpdate();
	}

	public void SetAimIKWeight(float weight)
	{
		aimIK.solver.IKPositionWeight = weight;
	}

	public void SetAimPosition(Vector3 pos)
	{
		aimTarget.position = pos;
	}

	public void SetAimRelativePosition(Vector3 pos)
	{
		aimTarget.position = references.root.root.position + pos;
	}

	public bool IsUsingAimIK()
	{
		if ((ikMode & IKMode.Aim) != IKMode.None)
		{
			return aimIK.solver.IKPositionWeight > 0.05f;
		}
		return false;
	}

	public bool GetAimIKForward(out Vector3 forward)
	{
		if (IsUsingAimIK() && aimIK.solver.transform != null)
		{
			forward = aimIK.solver.transformAxis;
			return true;
		}
		forward = base.transform.forward;
		return false;
	}

	public void SetLookIKWeight(float weight)
	{
		lookAtIK.solver.SetLookAtWeight(Mathf.Lerp(0f, lookAtWeight, weight));
	}

	public void SetUpdateAimAxis(bool update)
	{
		updateAimAxis = update;
		if (!updateAimAxis)
		{
			aimIK.solver.axis = defaultAimAxis;
		}
	}

	public void SetLookAtPosition(Vector3 pos)
	{
		lookTarget.position = pos;
	}

	public void SetFootIKWeight(float weight)
	{
		grounderBipedIK.weight = weight;
	}

	public void SetOffhandWeight(float weight)
	{
		fullBodyBipedIK.solver.leftHandEffector.positionWeight = (fullBodyBipedIK.solver.leftHandEffector.rotationWeight = weight);
	}

	public void WeaponReset()
	{
		SetAimTransform(FindChild(base.transform.GetComponentsInChildren<Transform>(), "Barrel_Point"));
		if (aimIK.solver.transform == null)
		{
			RemoveIKMode(IKMode.Aim);
		}
	}

	[ContextMenu("Initialize")]
	public void Initialize()
	{
		Transform[] componentsInChildren = references.root.GetComponentsInChildren<Transform>();
		references.root = base.transform;
		references.pelvis = FindChild(componentsInChildren, "Rig.Pelvis");
		references.leftThigh = FindChild(componentsInChildren, "Rig.Thigh.L");
		references.leftCalf = FindChild(componentsInChildren, "Rig.Calf.L");
		references.leftFoot = FindChild(componentsInChildren, "Rig.Foot.L");
		references.rightThigh = FindChild(componentsInChildren, "Rig.Thigh.R");
		references.rightCalf = FindChild(componentsInChildren, "Rig.Calf.R");
		references.rightFoot = FindChild(componentsInChildren, "Rig.Foot.R");
		references.leftUpperArm = FindChild(componentsInChildren, "Rig.UpperArm.L");
		references.leftForearm = FindChild(componentsInChildren, "Rig.Forearm.L");
		references.leftHand = FindChild(componentsInChildren, "Rig.AttachPoint.Hand.L");
		references.rightUpperArm = FindChild(componentsInChildren, "Rig.UpperArm.R");
		references.rightForearm = FindChild(componentsInChildren, "Rig.Forearm.R");
		references.rightHand = FindChild(componentsInChildren, "Rig.AttachPoint.Hand.R");
		references.head = FindChild(componentsInChildren, "Rig.Head");
		references.spine = new Transform[3]
		{
			FindChild(componentsInChildren, "Rig.Spine.01"),
			FindChild(componentsInChildren, "Rig.Spine.02"),
			FindChild(componentsInChildren, "Rig.Neck")
		};
		if (aimIK.solver.target != null)
		{
			aimTarget = aimIK.solver.target;
		}
		if (aimTarget == null)
		{
			aimTarget = base.transform.Find("AIM");
		}
		if (aimTarget == null)
		{
			aimTarget = new GameObject("AIM").transform;
		}
		aimTarget.SetParent(null);
		aimIK.solver.target = aimTarget;
		Transform[] hierarchy = new Transform[2]
		{
			references.spine[0],
			references.spine[1]
		};
		SetAimTransform(FindChild(componentsInChildren, "Barrel_Point"));
		aimIK.solver.SetChain(hierarchy, RootNode);
		aimIK.solver.bones[0].weight = 1f;
		aimIK.solver.bones[1].weight = 1f;
		if (lookTarget == null)
		{
			lookTarget = new GameObject("Look").transform;
		}
		lookTarget.SetParent(null);
		Transform[] spine = new Transform[1] { references.spine[2] };
		lookAtIK.solver.target = lookTarget;
		lookAtIK.solver.SetChain(spine, references.head, null, RootNode);
		lookAtIK.solver.SetLookAtWeight(lookAtWeight);
		lookAtIK.solver.headWeight = lookAtHeadWeight;
		lookAtIK.solver.spineWeightCurve = lookAtSpineWeight;
		fullBodyBipedIK.SetReferences(references, RootNode);
		fullBodyBipedIK.solver.SetChainWeights(FullBodyBipedChain.LeftArm, 0f);
		SetIKMode(ikMode);
	}

	public void SetIKMode(IKMode newMode)
	{
		ikMode = newMode;
		solverUpdate = delegate
		{
		};
		if (newMode.Contains(IKMode.Look))
		{
			solverUpdate = (Action)Delegate.Combine(solverUpdate, new Action(lookAtIK.solver.Update));
		}
		if (newMode.Contains(IKMode.Aim))
		{
			solverUpdate = (Action)Delegate.Combine(solverUpdate, new Action(aimIK.solver.Update));
		}
		if (newMode.Contains(IKMode.Ground))
		{
			solverUpdate = (Action)Delegate.Combine(solverUpdate, new Action(grounderBipedIK.solver.Update));
		}
		if (newMode.Contains(IKMode.FullBody))
		{
			solverUpdate = (Action)Delegate.Combine(solverUpdate, new Action(fullBodyBipedIK.solver.Update));
		}
	}

	public void AddIKMode(IKMode newMode)
	{
		ikMode |= newMode;
		SetIKMode(ikMode);
	}

	public void RemoveIKMode(IKMode oldMode)
	{
		ikMode ^= oldMode;
		SetIKMode(ikMode);
	}

	public void OnEquippedItemChanged(Item item, bool equipped)
	{
		VisualEquipmentSlot equipmentVisualSlot = item.EquipmentVisualSlot;
		if ((uint)equipmentVisualSlot <= 2u)
		{
			if (item.InventorySlot == Item.InventorySlotType.Major)
			{
				majorItem = (equipped ? item : null);
				rangedMajorItem = majorItem as RangedWeaponItem;
			}
			else
			{
				minorItem = (equipped ? item : null);
			}
			OnEquipmentOrActiveSlotChanged();
		}
	}

	public void SetActiveEquipmentSlot(int slot)
	{
		if (slot != lastSlotInUse)
		{
			lastSlotInUse = slot;
			OnEquipmentOrActiveSlotChanged();
		}
	}

	public bool AllowIKInAir()
	{
		if (lastSlotInUse == 0 && rangedMajorItem != null)
		{
			return (rangedMajorItem.InventoryUsableDefinition as RangedAttackUsableDefinition).CanShootInAir;
		}
		return false;
	}

	private void OnEquipmentOrActiveSlotChanged()
	{
		switch (lastSlotInUse)
		{
		case -2:
		case -1:
			SetAimTransform(null);
			SetIKMode(IKMode.None);
			break;
		case 0:
			SetAimTransform(GetItemTransform(majorItem));
			aimIK.solver.axis = defaultAimAxis;
			SetIKMode(IKMode.Aim);
			break;
		case 1:
			SetAimTransform(references.leftHand);
			aimIK.solver.axis = leftHandAxis;
			SetIKMode(IKMode.None);
			break;
		default:
			Debug.LogError($"Unknown equipment slot: {lastSlotInUse}");
			break;
		}
	}

	private Transform FindChild(Transform[] children, string childName)
	{
		foreach (Transform transform in children)
		{
			if (transform.name == childName)
			{
				return transform;
			}
		}
		return null;
	}

	private Transform GetItemTransform(Item item)
	{
		if (item == null)
		{
			return null;
		}
		return (item as RangedWeaponItem)?.ProjectileShooter.FirePoint ?? item.transform;
	}

	private void SetAimTransform(Transform t)
	{
		if (t == null)
		{
			t = references.rightHand;
		}
		aimIK.solver.transform = t;
	}
}

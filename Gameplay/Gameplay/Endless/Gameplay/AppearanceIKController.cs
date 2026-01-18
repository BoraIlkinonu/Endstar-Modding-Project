using System;
using Endless.Gameplay.PlayerInventory;
using Endless.Shared.DataTypes;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000268 RID: 616
	public class AppearanceIKController : MonoBehaviour
	{
		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000CA6 RID: 3238 RVA: 0x0004454C File Offset: 0x0004274C
		public BipedReferences References
		{
			get
			{
				return this.references;
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x00044554 File Offset: 0x00042754
		public Transform RootNode
		{
			get
			{
				return this.references.spine[0];
			}
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000CA8 RID: 3240 RVA: 0x00044563 File Offset: 0x00042763
		public AppearanceIKController.IKMode CurrentIKMode
		{
			get
			{
				return this.ikMode;
			}
		}

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x0004456B File Offset: 0x0004276B
		public int LastSlotInUse
		{
			get
			{
				return this.lastSlotInUse;
			}
		}

		// Token: 0x06000CAA RID: 3242 RVA: 0x00044574 File Offset: 0x00042774
		private void Awake()
		{
			this.lookAtIK.enabled = (this.aimIK.enabled = (this.grounderBipedIK.enabled = (this.fullBodyBipedIK.enabled = false)));
			this.SetIKMode(AppearanceIKController.IKMode.None);
			this.updateAimAxis = true;
		}

		// Token: 0x06000CAB RID: 3243 RVA: 0x000445C5 File Offset: 0x000427C5
		private void OnDestroy()
		{
			if (this.aimTarget != null)
			{
				global::UnityEngine.Object.Destroy(this.aimTarget.gameObject);
			}
			if (this.lookTarget != null)
			{
				global::UnityEngine.Object.Destroy(this.lookTarget.gameObject);
			}
		}

		// Token: 0x06000CAC RID: 3244 RVA: 0x00044603 File Offset: 0x00042803
		private void Update()
		{
			this.aimIK.solver.FixTransforms();
		}

		// Token: 0x06000CAD RID: 3245 RVA: 0x00044618 File Offset: 0x00042818
		private void LateUpdate()
		{
			if (this.aimIK == null || this.aimIK.solver == null)
			{
				string text = "aimik null? {0} solver? {1}";
				object obj = this.aimIK == null;
				AimIK aimIK = this.aimIK;
				Debug.LogError(string.Format(text, obj, ((aimIK != null) ? aimIK.solver : null) == null));
				return;
			}
			if (this.updateAimAxis && this.aimIK.solver.transform != null)
			{
				this.aimIK.solver.axis = this.aimIK.solver.transform.InverseTransformDirection(base.transform.forward);
			}
			this.solverUpdate();
		}

		// Token: 0x06000CAE RID: 3246 RVA: 0x000446D6 File Offset: 0x000428D6
		public void SetAimIKWeight(float weight)
		{
			this.aimIK.solver.IKPositionWeight = weight;
		}

		// Token: 0x06000CAF RID: 3247 RVA: 0x000446E9 File Offset: 0x000428E9
		public void SetAimPosition(Vector3 pos)
		{
			this.aimTarget.position = pos;
		}

		// Token: 0x06000CB0 RID: 3248 RVA: 0x000446F7 File Offset: 0x000428F7
		public void SetAimRelativePosition(Vector3 pos)
		{
			this.aimTarget.position = this.references.root.root.position + pos;
		}

		// Token: 0x06000CB1 RID: 3249 RVA: 0x0004471F File Offset: 0x0004291F
		public bool IsUsingAimIK()
		{
			return (this.ikMode & AppearanceIKController.IKMode.Aim) != AppearanceIKController.IKMode.None && this.aimIK.solver.IKPositionWeight > 0.05f;
		}

		// Token: 0x06000CB2 RID: 3250 RVA: 0x00044744 File Offset: 0x00042944
		public bool GetAimIKForward(out Vector3 forward)
		{
			if (this.IsUsingAimIK() && this.aimIK.solver.transform != null)
			{
				forward = this.aimIK.solver.transformAxis;
				return true;
			}
			forward = base.transform.forward;
			return false;
		}

		// Token: 0x06000CB3 RID: 3251 RVA: 0x0004479B File Offset: 0x0004299B
		public void SetLookIKWeight(float weight)
		{
			this.lookAtIK.solver.SetLookAtWeight(Mathf.Lerp(0f, this.lookAtWeight, weight));
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x000447BE File Offset: 0x000429BE
		public void SetUpdateAimAxis(bool update)
		{
			this.updateAimAxis = update;
			if (!this.updateAimAxis)
			{
				this.aimIK.solver.axis = this.defaultAimAxis;
			}
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x000447E5 File Offset: 0x000429E5
		public void SetLookAtPosition(Vector3 pos)
		{
			this.lookTarget.position = pos;
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x000447F3 File Offset: 0x000429F3
		public void SetFootIKWeight(float weight)
		{
			this.grounderBipedIK.weight = weight;
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x00044804 File Offset: 0x00042A04
		public void SetOffhandWeight(float weight)
		{
			IKEffector leftHandEffector = this.fullBodyBipedIK.solver.leftHandEffector;
			this.fullBodyBipedIK.solver.leftHandEffector.rotationWeight = weight;
			leftHandEffector.positionWeight = weight;
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0004483F File Offset: 0x00042A3F
		public void WeaponReset()
		{
			this.SetAimTransform(this.FindChild(base.transform.GetComponentsInChildren<Transform>(), "Barrel_Point"));
			if (this.aimIK.solver.transform == null)
			{
				this.RemoveIKMode(AppearanceIKController.IKMode.Aim);
			}
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x0004487C File Offset: 0x00042A7C
		[ContextMenu("Initialize")]
		public void Initialize()
		{
			Transform[] componentsInChildren = this.references.root.GetComponentsInChildren<Transform>();
			this.references.root = base.transform;
			this.references.pelvis = this.FindChild(componentsInChildren, "Rig.Pelvis");
			this.references.leftThigh = this.FindChild(componentsInChildren, "Rig.Thigh.L");
			this.references.leftCalf = this.FindChild(componentsInChildren, "Rig.Calf.L");
			this.references.leftFoot = this.FindChild(componentsInChildren, "Rig.Foot.L");
			this.references.rightThigh = this.FindChild(componentsInChildren, "Rig.Thigh.R");
			this.references.rightCalf = this.FindChild(componentsInChildren, "Rig.Calf.R");
			this.references.rightFoot = this.FindChild(componentsInChildren, "Rig.Foot.R");
			this.references.leftUpperArm = this.FindChild(componentsInChildren, "Rig.UpperArm.L");
			this.references.leftForearm = this.FindChild(componentsInChildren, "Rig.Forearm.L");
			this.references.leftHand = this.FindChild(componentsInChildren, "Rig.AttachPoint.Hand.L");
			this.references.rightUpperArm = this.FindChild(componentsInChildren, "Rig.UpperArm.R");
			this.references.rightForearm = this.FindChild(componentsInChildren, "Rig.Forearm.R");
			this.references.rightHand = this.FindChild(componentsInChildren, "Rig.AttachPoint.Hand.R");
			this.references.head = this.FindChild(componentsInChildren, "Rig.Head");
			this.references.spine = new Transform[]
			{
				this.FindChild(componentsInChildren, "Rig.Spine.01"),
				this.FindChild(componentsInChildren, "Rig.Spine.02"),
				this.FindChild(componentsInChildren, "Rig.Neck")
			};
			if (this.aimIK.solver.target != null)
			{
				this.aimTarget = this.aimIK.solver.target;
			}
			if (this.aimTarget == null)
			{
				this.aimTarget = base.transform.Find("AIM");
			}
			if (this.aimTarget == null)
			{
				this.aimTarget = new GameObject("AIM").transform;
			}
			this.aimTarget.SetParent(null);
			this.aimIK.solver.target = this.aimTarget;
			Transform[] array = new Transform[]
			{
				this.references.spine[0],
				this.references.spine[1]
			};
			this.SetAimTransform(this.FindChild(componentsInChildren, "Barrel_Point"));
			this.aimIK.solver.SetChain(array, this.RootNode);
			this.aimIK.solver.bones[0].weight = 1f;
			this.aimIK.solver.bones[1].weight = 1f;
			if (this.lookTarget == null)
			{
				this.lookTarget = new GameObject("Look").transform;
			}
			this.lookTarget.SetParent(null);
			Transform[] array2 = new Transform[] { this.references.spine[2] };
			this.lookAtIK.solver.target = this.lookTarget;
			this.lookAtIK.solver.SetChain(array2, this.references.head, null, this.RootNode);
			this.lookAtIK.solver.SetLookAtWeight(this.lookAtWeight);
			this.lookAtIK.solver.headWeight = this.lookAtHeadWeight;
			this.lookAtIK.solver.spineWeightCurve = this.lookAtSpineWeight;
			this.fullBodyBipedIK.SetReferences(this.references, this.RootNode);
			this.fullBodyBipedIK.solver.SetChainWeights(FullBodyBipedChain.LeftArm, 0f, 0f);
			this.SetIKMode(this.ikMode);
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x00044C4C File Offset: 0x00042E4C
		public void SetIKMode(AppearanceIKController.IKMode newMode)
		{
			this.ikMode = newMode;
			this.solverUpdate = delegate
			{
			};
			if (newMode.Contains(AppearanceIKController.IKMode.Look))
			{
				this.solverUpdate = (Action)Delegate.Combine(this.solverUpdate, new Action(this.lookAtIK.solver.Update));
			}
			if (newMode.Contains(AppearanceIKController.IKMode.Aim))
			{
				this.solverUpdate = (Action)Delegate.Combine(this.solverUpdate, new Action(this.aimIK.solver.Update));
			}
			if (newMode.Contains(AppearanceIKController.IKMode.Ground))
			{
				this.solverUpdate = (Action)Delegate.Combine(this.solverUpdate, new Action(this.grounderBipedIK.solver.Update));
			}
			if (newMode.Contains(AppearanceIKController.IKMode.FullBody))
			{
				this.solverUpdate = (Action)Delegate.Combine(this.solverUpdate, new Action(this.fullBodyBipedIK.solver.Update));
			}
		}

		// Token: 0x06000CBB RID: 3259 RVA: 0x00044D59 File Offset: 0x00042F59
		public void AddIKMode(AppearanceIKController.IKMode newMode)
		{
			this.ikMode |= newMode;
			this.SetIKMode(this.ikMode);
		}

		// Token: 0x06000CBC RID: 3260 RVA: 0x00044D75 File Offset: 0x00042F75
		public void RemoveIKMode(AppearanceIKController.IKMode oldMode)
		{
			this.ikMode ^= oldMode;
			this.SetIKMode(this.ikMode);
		}

		// Token: 0x06000CBD RID: 3261 RVA: 0x00044D94 File Offset: 0x00042F94
		public void OnEquippedItemChanged(Item item, bool equipped)
		{
			VisualEquipmentSlot equipmentVisualSlot = item.EquipmentVisualSlot;
			if (equipmentVisualSlot > VisualEquipmentSlot.BothHands)
			{
				return;
			}
			if (item.InventorySlot == Item.InventorySlotType.Major)
			{
				this.majorItem = (equipped ? item : null);
				this.rangedMajorItem = this.majorItem as RangedWeaponItem;
			}
			else
			{
				this.minorItem = (equipped ? item : null);
			}
			this.OnEquipmentOrActiveSlotChanged();
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x00044DE8 File Offset: 0x00042FE8
		public void SetActiveEquipmentSlot(int slot)
		{
			if (slot == this.lastSlotInUse)
			{
				return;
			}
			this.lastSlotInUse = slot;
			this.OnEquipmentOrActiveSlotChanged();
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x00044E01 File Offset: 0x00043001
		public bool AllowIKInAir()
		{
			return this.lastSlotInUse == 0 && this.rangedMajorItem != null && (this.rangedMajorItem.InventoryUsableDefinition as RangedAttackUsableDefinition).CanShootInAir;
		}

		// Token: 0x06000CC0 RID: 3264 RVA: 0x00044E30 File Offset: 0x00043030
		private void OnEquipmentOrActiveSlotChanged()
		{
			switch (this.lastSlotInUse)
			{
			case -2:
			case -1:
				this.SetAimTransform(null);
				this.SetIKMode(AppearanceIKController.IKMode.None);
				return;
			case 0:
				this.SetAimTransform(this.GetItemTransform(this.majorItem));
				this.aimIK.solver.axis = this.defaultAimAxis;
				this.SetIKMode(AppearanceIKController.IKMode.Aim);
				return;
			case 1:
				this.SetAimTransform(this.references.leftHand);
				this.aimIK.solver.axis = this.leftHandAxis;
				this.SetIKMode(AppearanceIKController.IKMode.None);
				return;
			default:
				Debug.LogError(string.Format("Unknown equipment slot: {0}", this.lastSlotInUse));
				return;
			}
		}

		// Token: 0x06000CC1 RID: 3265 RVA: 0x00044EE8 File Offset: 0x000430E8
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

		// Token: 0x06000CC2 RID: 3266 RVA: 0x00044F1A File Offset: 0x0004311A
		private Transform GetItemTransform(Item item)
		{
			if (item == null)
			{
				return null;
			}
			RangedWeaponItem rangedWeaponItem = item as RangedWeaponItem;
			return ((rangedWeaponItem != null) ? rangedWeaponItem.ProjectileShooter.FirePoint : null) ?? item.transform;
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x00044F48 File Offset: 0x00043148
		private void SetAimTransform(Transform t)
		{
			if (t == null)
			{
				t = this.references.rightHand;
			}
			this.aimIK.solver.transform = t;
		}

		// Token: 0x04000BBB RID: 3003
		private const string GUN_AIM_TRANSFORM_NAME = "Barrel_Point";

		// Token: 0x04000BBC RID: 3004
		[SerializeField]
		private FullBodyBipedIK fullBodyBipedIK;

		// Token: 0x04000BBD RID: 3005
		[SerializeField]
		private GrounderFBBIK grounderBipedIK;

		// Token: 0x04000BBE RID: 3006
		[SerializeField]
		private AimIK aimIK;

		// Token: 0x04000BBF RID: 3007
		[SerializeField]
		private LookAtIK lookAtIK;

		// Token: 0x04000BC0 RID: 3008
		[SerializeField]
		private BipedReferences references;

		// Token: 0x04000BC1 RID: 3009
		[SerializeField]
		private float lookAtWeight = 0.943f;

		// Token: 0x04000BC2 RID: 3010
		[SerializeField]
		private float lookAtHeadWeight = 0.891f;

		// Token: 0x04000BC3 RID: 3011
		[SerializeField]
		private AnimationCurve lookAtSpineWeight = new AnimationCurve();

		// Token: 0x04000BC4 RID: 3012
		[NonSerialized]
		private Transform aimTarget;

		// Token: 0x04000BC5 RID: 3013
		[NonSerialized]
		private Transform lookTarget;

		// Token: 0x04000BC6 RID: 3014
		[NonSerialized]
		private Vector3 defaultAimAxis = Vector3.forward;

		// Token: 0x04000BC7 RID: 3015
		[NonSerialized]
		private AppearanceIKController.IKMode ikMode;

		// Token: 0x04000BC8 RID: 3016
		[SerializeField]
		private bool updateAimAxis;

		// Token: 0x04000BC9 RID: 3017
		[NonSerialized]
		private SerializableGuid equippedGuid;

		// Token: 0x04000BCA RID: 3018
		[NonSerialized]
		private Item majorItem;

		// Token: 0x04000BCB RID: 3019
		[NonSerialized]
		private Item minorItem;

		// Token: 0x04000BCC RID: 3020
		[NonSerialized]
		private RangedWeaponItem rangedMajorItem;

		// Token: 0x04000BCD RID: 3021
		[NonSerialized]
		private int lastSlotInUse = -2;

		// Token: 0x04000BCE RID: 3022
		[SerializeField]
		private Vector3 leftHandAxis = Vector3.left;

		// Token: 0x04000BCF RID: 3023
		[NonSerialized]
		private Action solverUpdate = delegate
		{
		};

		// Token: 0x02000269 RID: 617
		[Flags]
		public enum IKMode : byte
		{
			// Token: 0x04000BD1 RID: 3025
			None = 0,
			// Token: 0x04000BD2 RID: 3026
			Look = 1,
			// Token: 0x04000BD3 RID: 3027
			Aim = 2,
			// Token: 0x04000BD4 RID: 3028
			Ground = 4,
			// Token: 0x04000BD5 RID: 3029
			FullBody = 8
		}
	}
}

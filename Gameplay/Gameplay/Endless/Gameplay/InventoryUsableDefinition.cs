using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002E5 RID: 741
	public abstract class InventoryUsableDefinition : UsableDefinition
	{
		// Token: 0x1700034C RID: 844
		// (get) Token: 0x060010C0 RID: 4288 RVA: 0x00054F3C File Offset: 0x0005313C
		public InventoryUsableDefinition.InventoryTypes InventoryType
		{
			get
			{
				return this.inventoryType;
			}
		}

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x060010C1 RID: 4289 RVA: 0x00054F44 File Offset: 0x00053144
		public bool IsStackable
		{
			get
			{
				return this.isStackable;
			}
		}

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x060010C2 RID: 4290 RVA: 0x00054F4C File Offset: 0x0005314C
		public InventoryUsableDefinition.MobileUI_MajorLayoutType MobileUIMajorLayout
		{
			get
			{
				return this.mobileMajorLayout;
			}
		}

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x060010C3 RID: 4291 RVA: 0x00054F54 File Offset: 0x00053154
		public InventoryUsableDefinition.MobileUI_MinorLayoutType MobileUIMinorLayout
		{
			get
			{
				return this.mobileMinorLayout;
			}
		}

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x060010C4 RID: 4292 RVA: 0x00054F5C File Offset: 0x0005315C
		public string AnimationTrigger
		{
			get
			{
				return this.animationTrigger;
			}
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x00054F64 File Offset: 0x00053164
		public virtual string GetAnimationTrigger(UsableDefinition.UseState eus, uint currentVisualFrame)
		{
			return this.AnimationTrigger;
		}

		// Token: 0x060010C6 RID: 4294 RVA: 0x0001BD04 File Offset: 0x00019F04
		public virtual InventoryUsableDefinition.EquipmentShowPriority GetShowPriority(UsableDefinition.UseState eus)
		{
			return InventoryUsableDefinition.EquipmentShowPriority.Major;
		}

		// Token: 0x060010C7 RID: 4295 RVA: 0x00054F6C File Offset: 0x0005316C
		public virtual void GetEventData(NetState state, UsableDefinition.UseState useState, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			data.Reset();
		}

		// Token: 0x04000E7B RID: 3707
		[SerializeField]
		private InventoryUsableDefinition.InventoryTypes inventoryType = InventoryUsableDefinition.InventoryTypes.Minor;

		// Token: 0x04000E7C RID: 3708
		[SerializeField]
		private bool isStackable;

		// Token: 0x04000E7D RID: 3709
		[SerializeField]
		[Header("--")]
		private InventoryUsableDefinition.MobileUI_MajorLayoutType mobileMajorLayout;

		// Token: 0x04000E7E RID: 3710
		[SerializeField]
		private InventoryUsableDefinition.MobileUI_MinorLayoutType mobileMinorLayout;

		// Token: 0x04000E7F RID: 3711
		[SerializeField]
		[Header("--")]
		private string animationTrigger;

		// Token: 0x020002E6 RID: 742
		public enum InventoryTypes
		{
			// Token: 0x04000E81 RID: 3713
			Major,
			// Token: 0x04000E82 RID: 3714
			Minor
		}

		// Token: 0x020002E7 RID: 743
		public enum EquipmentShowPriority
		{
			// Token: 0x04000E84 RID: 3716
			NotShown,
			// Token: 0x04000E85 RID: 3717
			MinorOutOfUse,
			// Token: 0x04000E86 RID: 3718
			Major,
			// Token: 0x04000E87 RID: 3719
			MinorInUse
		}

		// Token: 0x020002E8 RID: 744
		public enum MobileUI_MajorLayoutType
		{
			// Token: 0x04000E89 RID: 3721
			Generic,
			// Token: 0x04000E8A RID: 3722
			Melee,
			// Token: 0x04000E8B RID: 3723
			Ranged
		}

		// Token: 0x020002E9 RID: 745
		public enum MobileUI_MinorLayoutType
		{
			// Token: 0x04000E8D RID: 3725
			Button,
			// Token: 0x04000E8E RID: 3726
			ButtonWithJoystick
		}

		// Token: 0x020002EA RID: 746
		public struct EventData
		{
			// Token: 0x060010C9 RID: 4297 RVA: 0x00054F84 File Offset: 0x00053184
			public void Reset()
			{
				this.InUse = false;
				this.Available = true;
				this.CooldownSecondsLeft = 0f;
				this.CooldownSecondsTotal = 0f;
				this.ResourcePercent = 1f;
				this.UseFrame = 0U;
			}

			// Token: 0x060010CA RID: 4298 RVA: 0x00054FBC File Offset: 0x000531BC
			public void CopyTo(ref InventoryUsableDefinition.EventData target)
			{
				target.InUse = this.InUse;
				target.Available = this.Available;
				target.CooldownSecondsLeft = this.CooldownSecondsLeft;
				target.CooldownSecondsTotal = this.CooldownSecondsTotal;
				target.ResourcePercent = this.ResourcePercent;
				target.UseFrame = this.UseFrame;
			}

			// Token: 0x04000E8F RID: 3727
			public bool InUse;

			// Token: 0x04000E90 RID: 3728
			public bool Available;

			// Token: 0x04000E91 RID: 3729
			public float CooldownSecondsLeft;

			// Token: 0x04000E92 RID: 3730
			public float CooldownSecondsTotal;

			// Token: 0x04000E93 RID: 3731
			public float ResourcePercent;

			// Token: 0x04000E94 RID: 3732
			public uint UseFrame;
		}
	}
}

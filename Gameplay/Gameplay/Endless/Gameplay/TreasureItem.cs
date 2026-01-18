using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002BA RID: 698
	public class TreasureItem : Item
	{
		// Token: 0x17000320 RID: 800
		// (get) Token: 0x06000FE9 RID: 4073 RVA: 0x00051BED File Offset: 0x0004FDED
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000FEA RID: 4074 RVA: 0x00051BF5 File Offset: 0x0004FDF5
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x00051C00 File Offset: 0x0004FE00
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000FED RID: 4077 RVA: 0x0004F016 File Offset: 0x0004D216
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000FEE RID: 4078 RVA: 0x00051C16 File Offset: 0x0004FE16
		protected internal override string __getTypeName()
		{
			return "TreasureItem";
		}

		// Token: 0x04000DC0 RID: 3520
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000DC1 RID: 3521
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;
	}
}

using System;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200032A RID: 810
	public class Key : Item
	{
		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x060012CD RID: 4813 RVA: 0x0005C6B4 File Offset: 0x0005A8B4
		public override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic | ReferenceFilter.InventoryItem | ReferenceFilter.Key;
			}
		}

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x060012CE RID: 4814 RVA: 0x0005C6B8 File Offset: 0x0005A8B8
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.groundVisualsInfo;
			}
		}

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x060012CF RID: 4815 RVA: 0x0005C6C0 File Offset: 0x0005A8C0
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.equippedVisualsInfo;
			}
		}

		// Token: 0x060012D0 RID: 4816 RVA: 0x0005C6C8 File Offset: 0x0005A8C8
		public GameObject GetLockVisuals()
		{
			return base.GetComponent<KeyReferences>().LockVisuals;
		}

		// Token: 0x060012D2 RID: 4818 RVA: 0x0005C6D8 File Offset: 0x0005A8D8
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060012D3 RID: 4819 RVA: 0x0004F016 File Offset: 0x0004D216
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060012D4 RID: 4820 RVA: 0x0005C6EE File Offset: 0x0005A8EE
		protected internal override string __getTypeName()
		{
			return "Key";
		}

		// Token: 0x04001000 RID: 4096
		[SerializeField]
		private Item.VisualsInfo groundVisualsInfo;

		// Token: 0x04001001 RID: 4097
		[SerializeField]
		private Item.VisualsInfo equippedVisualsInfo;
	}
}

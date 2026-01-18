using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003B4 RID: 948
	[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/PropCreationMenuData")]
	public class PropCreationMenuData : PropCreationData
	{
		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06001282 RID: 4738 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		public override bool IsSubMenu
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06001283 RID: 4739 RVA: 0x0005F5CA File Offset: 0x0005D7CA
		public List<PropCreationData> Options
		{
			get
			{
				return this.options;
			}
		}

		// Token: 0x04000F47 RID: 3911
		[SerializeField]
		private List<PropCreationData> options = new List<PropCreationData>();
	}
}

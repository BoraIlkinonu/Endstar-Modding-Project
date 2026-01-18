using System;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003B3 RID: 947
	public abstract class PropCreationData : ScriptableObject
	{
		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x0600127E RID: 4734
		public abstract bool IsSubMenu { get; }

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x0600127F RID: 4735 RVA: 0x0005F5BA File Offset: 0x0005D7BA
		public Sprite Icon
		{
			get
			{
				return this.icon;
			}
		}

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06001280 RID: 4736 RVA: 0x0005F5C2 File Offset: 0x0005D7C2
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
		}

		// Token: 0x04000F45 RID: 3909
		[SerializeField]
		private string displayName;

		// Token: 0x04000F46 RID: 3910
		[SerializeField]
		private Sprite icon;
	}
}

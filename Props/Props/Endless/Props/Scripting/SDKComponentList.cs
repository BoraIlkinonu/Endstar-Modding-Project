using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Props.Scripting
{
	// Token: 0x0200000F RID: 15
	[CreateAssetMenu(menuName = "ScriptableObject/SDK/SDK Component List")]
	public class SDKComponentList : ScriptableObject
	{
		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002971 File Offset: 0x00000B71
		public IReadOnlyList<ComponentListEntry> ComponentListEntries
		{
			get
			{
				return this.componentListEntries;
			}
		}

		// Token: 0x04000030 RID: 48
		[SerializeField]
		private List<ComponentListEntry> componentListEntries = new List<ComponentListEntry>();
	}
}

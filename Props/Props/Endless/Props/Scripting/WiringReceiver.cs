using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Props.Scripting
{
	// Token: 0x02000010 RID: 16
	[Serializable]
	public class WiringReceiver
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600003F RID: 63 RVA: 0x0000298C File Offset: 0x00000B8C
		// (set) Token: 0x06000040 RID: 64 RVA: 0x00002994 File Offset: 0x00000B94
		public string Name { get; private set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000041 RID: 65 RVA: 0x0000299D File Offset: 0x00000B9D
		public IReadOnlyList<InspectorScriptValue> Arguments
		{
			get
			{
				return this.arguments;
			}
		}

		// Token: 0x04000032 RID: 50
		[SerializeField]
		private List<InspectorScriptValue> arguments = new List<InspectorScriptValue>();
	}
}

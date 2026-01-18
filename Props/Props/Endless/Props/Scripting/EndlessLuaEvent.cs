using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Props.Scripting
{
	// Token: 0x0200000A RID: 10
	[Serializable]
	public class EndlessLuaEvent
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000021 RID: 33 RVA: 0x0000267F File Offset: 0x0000087F
		// (set) Token: 0x06000022 RID: 34 RVA: 0x00002687 File Offset: 0x00000887
		public string Name { get; private set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002690 File Offset: 0x00000890
		public IReadOnlyList<InspectorScriptValue> Arguments
		{
			get
			{
				return this.arguments;
			}
		}

		// Token: 0x0400001F RID: 31
		[SerializeField]
		private List<InspectorScriptValue> arguments = new List<InspectorScriptValue>();
	}
}
